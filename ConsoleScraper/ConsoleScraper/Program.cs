using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using ConsoleScraper.Models;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Configuration;
using ConsoleScraper.Enums;
using OfficeOpenXml;

namespace ConsoleScraper
{
	/*
		TODO: Make config parameters for constants eg path, whether to make local files, whether to run against the local repo etc
		TODO: Make Excel file
		TODO: Make options for user prompts
		TODO: Support AirForces
	*/

	class Program
	{
		// Helper objects
		public static VehicleCostUnitHelper vehicleCostUnitHelper = new VehicleCostUnitHelper();
		public static VehicleSpeedUnitHelper vehicleSpeedUnitHelper = new VehicleSpeedUnitHelper();
		public static VehicleWeightUnitHelper vehicleWeightUnitHelper = new VehicleWeightUnitHelper();
		public static VehicleCountryHelper vehicleCountryHelper = new VehicleCountryHelper();
		public static GroundVehicleTypeHelper vehicleTypeHelper = new GroundVehicleTypeHelper();
		public static VehicleEnginePowerUnitHelper vehicleEnginePowerUnitHelper = new VehicleEnginePowerUnitHelper();

		public static List<string> errorList = new List<string>();

		/** Thread-safe collections **/
		// Populated with the vehicle name as the key and the HTML content of the page as the value
		public static ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent = new ConcurrentDictionary<string, HtmlDocument>();
		// Keeps track of changes made to local files, vehicle name as the key, and the action performed as the value
		public static ConcurrentDictionary<string, string> localFileChanges = new ConcurrentDictionary<string, string>();
		// Populated with the vehicle name and vehicle objects
		public static Dictionary<string, GroundVehicle> vehicleDetails = new Dictionary<string, GroundVehicle>();

		#region Debugging helpers

		public static Stopwatch overallStopwatch = new Stopwatch();
		public static Stopwatch webCrawlerStopwatch = new Stopwatch();
		public static Stopwatch processingStopwatch = new Stopwatch();
		public static Dictionary<string, int> propertyTotals = new Dictionary<string, int>();

		#endregion

		static void Main(string[] args)
		{
			try
			{
				overallStopwatch.Start();

				HtmlWeb webGet = new HtmlWeb();

				// Load Wiki Home page
				HtmlDocument groundForcesWikiHomePage = webGet.Load(ConfigurationManager.AppSettings["GroundForcesWikiUrl"]);

				// Fail fast if there are errors
				if (groundForcesWikiHomePage.ParseErrors != null && groundForcesWikiHomePage.ParseErrors.Any())
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("The following errors were encountered:");
					Console.WriteLine();

					foreach (HtmlParseError error in groundForcesWikiHomePage.ParseErrors)
					{
						Console.WriteLine(error.Reason);
					}

					Console.ResetColor();
				}
				else
				{
					// Setup initial vars
					int totalNumberOfLinksBasedOnPageText = 0;
					int totalNumberOfLinksBasedOnDomTraversal = 0;
					List <HtmlNode> vehicleWikiEntryLinks = new List<HtmlNode>();

					GetLinksToVehiclePages(vehicleWikiEntryLinks, groundForcesWikiHomePage, out totalNumberOfLinksBasedOnPageText, out totalNumberOfLinksBasedOnDomTraversal);

					// Setup thread-safe collections for processing
					ConcurrentDictionary<int, HtmlNode> linksToVehicleWikiPages = new ConcurrentDictionary<int, HtmlNode>();

					// Populate the full list of links we need to traverse
					for (int i = 0; i < vehicleWikiEntryLinks.Count(); i++)
					{
						HtmlNode linkNode = vehicleWikiEntryLinks[i];
						linksToVehicleWikiPages.TryAdd(i, linkNode);
					}

					webCrawlerStopwatch.Start();

					// Crawl the pages concurrently
					Task[] webCrawlerTasks = new Task[4]
					{
						// Going from 2 to 4 tasks halves the processing time, after 4 tasks the performance gain is negligible
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages)),
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages)),
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages)),
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages))
					};

					// Wait until we have crawled all of the pages
					Task.WaitAll(webCrawlerTasks);

					webCrawlerStopwatch.Stop();
					processingStopwatch.Start();

					int indexPosition = 1;

					// Extract information from the pages we've traversed
					ProcessWikiHtmlFiles(indexPosition, totalNumberOfLinksBasedOnPageText);

					processingStopwatch.Stop();

					Console.WriteLine("================================================================");

					// Write out errors
					if (errorList.Any())
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"The following error{(errorList.Count() > 1 ? "s were" : "was")} encountered:");

						foreach (string error in errorList)
						{
							Console.WriteLine(error);
						}
					}

					Console.ResetColor();
					Console.WriteLine("================================================================");
					Console.WriteLine();

					overallStopwatch.Stop();

					// Write out summary
					TimeSpan timeSpan = overallStopwatch.Elapsed;
					Console.WriteLine($"Completed in {timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}");
					Console.WriteLine($"Expected total: {totalNumberOfLinksBasedOnPageText}, Actual total: {totalNumberOfLinksBasedOnDomTraversal}");
					Console.WriteLine($"Vehicle objects created: {vehicleDetails.Count()} (should be Actual - Errors)");

					// Write out local file changes
					if(localFileChanges.Any())
					{
						Console.WriteLine();
						Console.WriteLine("The following changes were made to the local wiki files: ");

						Dictionary<string, string> orderedLocalFileChanges = localFileChanges.OrderBy(x => x.Key).ToDictionary(d => d.Key, d => d.Value);

						foreach(string change in orderedLocalFileChanges.Values)
						{
							Console.WriteLine(change);
						}
					}
				}

				// Wait until the user hits 'Esc' to terminate the application
				Console.WriteLine();
				Console.WriteLine("Press ESC to exit...");

				while (true)
				{
					ConsoleKeyInfo k = Console.ReadKey(true);

					if (k.Key == ConsoleKey.Escape)
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"The following exception was encounted: {ex.Message}");
				Console.WriteLine($"Exception details: {ex.StackTrace}");
			}
		}

		/// <summary>
		/// Finds all of the links to vehicles on the current page, then checks to see
		/// if there is a link to the next page, if there is then that page is loaded
		/// and this method is called recursively until all of the links have been
		/// gathered.
		/// </summary>
		/// <param name="vehicleWikiEntryLinks">The list to store the found links in</param>
		/// <param name="pageUrl">The url of the page to check for more links</param>
		/// <param name="totalNumberOfLinksBasedOnPageText">Used to store how many links we should expect</param>
		/// <param name="totalNumberOfLinksBasedOnDomTraversal">Used to store how many links we actually found</param>
		public static void GetLinksToVehiclePages(List<HtmlNode> vehicleWikiEntryLinks, HtmlDocument pageUrl, out int totalNumberOfLinksBasedOnPageText, out int totalNumberOfLinksBasedOnDomTraversal)
		{
			// Get "Pages in category "Ground vehicles"" section | <div id="mw-pages"> | document.getElementById('mw-pages')
			HtmlNode listContainerNode = pageUrl.DocumentNode.Descendants().Single(d => d.Id == "mw-pages");
			// Get container that holds the table with the links | <div lang="en" dir="ltr" class="mw-content-ltr"> | document.getElementsByClassName('mw-content-ltr')[1]
			HtmlNode tableContainerNode = listContainerNode.Descendants("div").Single(d => d.Attributes["class"].Value.Contains("mw-content-ltr"));

			// Get Vehicle links from the initial page | div > table > tbody > tr > td > ul > li > a | document.getElementsByClassName('mw-content-ltr')[1].getElementsByTagName('a')
			vehicleWikiEntryLinks.AddRange(tableContainerNode.Descendants("table").Single().Descendants("a").ToList());

			// Get totals for the number of links to expect, and the number found
			string totalEntriesTextBlock = listContainerNode.Descendants("p").Single().InnerText;
			MatchCollection matches = Regex.Matches(totalEntriesTextBlock, @"\d+");
			totalNumberOfLinksBasedOnPageText = int.Parse(matches[matches.Count - 1].Value);
			totalNumberOfLinksBasedOnDomTraversal = vehicleWikiEntryLinks.Count();

			// Get vehicle links from the subsequent pages | <a href="/index.php?title=Category:Ground_vehicles&amp;pagefrom=T-54+mod.+1949#mw-pages" title="Category:Ground vehicles">next 200</a> | document.querySelectorAll('#mw-pages a[Title="Category:Ground vehicles"]')[0]
			HtmlNode nextPageLink = listContainerNode.Descendants("a").Where(d => d.InnerText.Contains("next") && d.Attributes["title"].Value.Contains("Category:Ground vehicles")).FirstOrDefault();

			if(nextPageLink != null)
			{
				// Build the link for the next page
				Uri subsequentWikPage = new Uri(new Uri(ConfigurationManager.AppSettings["BaseWikiUrl"]), nextPageLink.Attributes["href"].Value);
				string subsequentPageUrl = System.Net.WebUtility.HtmlDecode(subsequentWikPage.ToString());

				// Load Wiki page
				HtmlWeb webGet = new HtmlWeb();
				HtmlDocument groundForcesWikiPage = webGet.Load(subsequentPageUrl);

				// Call this method
				GetLinksToVehiclePages(vehicleWikiEntryLinks, groundForcesWikiPage, out totalNumberOfLinksBasedOnPageText, out totalNumberOfLinksBasedOnDomTraversal);
			}
		}

		/// <summary>
		/// This is called inside the worker tasks to crawl the pages asynchronously
		/// </summary>
		/// <param name="vehiclePageLinks">A dictionary that contains a indexer for the key, and a link to the wiki page for that vehicle as the value</param>
		public static void GetPageHtml(ConcurrentDictionary<int, HtmlNode> vehiclePageLinks)
		{
			foreach (var vehiclePageLink in vehiclePageLinks)
			{
				// Remove the current node so that the other threads don't reprocess it
				HtmlNode tempNode;
				vehiclePageLinks.TryRemove(vehiclePageLink.Key, out tempNode);

				// Fetch page information
				HtmlNode linkNode = vehiclePageLink.Value;
				string wikiRelativeUrl = linkNode.Attributes.Single(l => l.Name == "href").Value;
				string vehicleWikiEntryFullUrl = new Uri(new Uri(ConfigurationManager.AppSettings["BaseWikiUrl"]), wikiRelativeUrl).ToString();
				string vehicleName = linkNode.InnerText;

				Console.WriteLine();
				Console.WriteLine($"Processing... {vehicleName}");
				Console.WriteLine($"Vehicle: {vehicleName}, Url: {vehicleWikiEntryFullUrl}");

				// Visit page and extract data
				HtmlWeb vehicleWebGet = new HtmlWeb();
				HtmlDocument vehicleWikiPage = vehicleWebGet.Load(vehicleWikiEntryFullUrl);

				// Add page to new dictionary used to extract further data
				vehicleWikiPagesContent.TryAdd(vehicleName, vehicleWikiPage);

				Console.WriteLine(vehicleWikiPagesContent.Count());
			}
		}

		/// <summary>
		/// Loops through all of the vehicle wiki links that have been provided, attempts to parse the parts that we are interested in -
		/// the vehicle details table, creates an object from that, then stores the data locally if a flag is set
		/// </summary>
		/// <param name="indexPosition">The current index we are up to processing - used for error messages</param>
		/// <param name="expectedNumberOfLinks">The expected number of links to process</param>
		private static void ProcessWikiHtmlFiles(int indexPosition, int expectedNumberOfLinks)
		{
			try
			{
				// Setup objects to handle creating the spreadsheet
				FileInfo excelFile = ConfigurationManager.AppSettings["UpdateExcelDocument"] == "True"
					? new FileInfo($"{ConfigurationManager.AppSettings["LocalWikiExcelPath"]}GroundVehicleData.xlsx")
					: null;
				ExcelPackage excelPackage = ConfigurationManager.AppSettings["UpdateExcelDocument"] == "True"
					? new ExcelPackage(excelFile)
					: null;
				ExcelWorksheet worksheet = ConfigurationManager.AppSettings["UpdateExcelDocument"] == "True"
					? excelPackage.Workbook.Worksheets.FirstOrDefault() == null
						? excelPackage.Workbook.Worksheets.Add("Data")
						: excelPackage.Workbook.Worksheets.Single(w => w.Name == "Data")
					: null;

				if (ConfigurationManager.AppSettings["UpdateExcelDocument"] == "True")
				{
					// Clear out old data before populating the headers again
					worksheet.DeleteColumn(1, 30);
					CreateGroundVehicleSpreadsheetHeaders(worksheet);
				}

				foreach (string vehicleWikiPageLinkTitle in vehicleWikiPagesContent.Keys)
				{
					// Page to traverse
					HtmlDocument vehicleWikiPage = vehicleWikiPagesContent.Single(x => x.Key == vehicleWikiPageLinkTitle).Value;
					// Get the header that holds the page title | document.getElementsByClassName('firstHeading')[0].firstChild.innerText
					HtmlNode pageTitle = vehicleWikiPage.DocumentNode.Descendants().Single(d => d.Id == "firstHeading").FirstChild;
					// Get the div that holds all of the content under the title section | document.getElementById('bodyContent')
					HtmlNode wikiBody = vehicleWikiPage.DocumentNode.Descendants().Single(d => d.Id == "bodyContent");
					// Get the div that holds the content on the RHS of the page where the information table is | document.getElementById('bodyContent').getElementsByClassName('right-area')
					HtmlNode rightHandContent =
						wikiBody.Descendants("div")
							.Single(d => d.Attributes["class"] != null && d.Attributes["class"].Value.Contains("right-area"));
					// Get the able that holds all of the vehicle information | document.getElementsByClassName('flight-parameters')[0]
					HtmlNode infoBox =
						rightHandContent.Descendants("table")
							.SingleOrDefault(d => d.Attributes["class"].Value.Contains("flight-parameters"));

					// Name
					string vehicleName = RemoveInvalidCharacters(System.Net.WebUtility.HtmlDecode(vehicleWikiPageLinkTitle));

					// Fail fast and create error if there is no info box
					if (infoBox == null)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"Error processing item {indexPosition} of {expectedNumberOfLinks}");
						Console.WriteLine();
						Console.ResetColor();

						errorList.Add($"No Information found for '{vehicleName}', proceeding to next vehicle");
						indexPosition++;
						continue;
					}
					else
					{
						// Setup local vars
						Dictionary<string, string> vehicleAttributes = new Dictionary<string, string>();
						HtmlNodeCollection rows = infoBox.SelectNodes("tr");

						Console.WriteLine($"The following values were found for {vehicleName}");

						GetAttributesFromInfoBox(vehicleAttributes, rows);

						Console.ResetColor();

						// Country
						string countryRawValue = vehicleAttributes.Single(k => k.Key == "Country").Value.ToString();
						CountryEnum vehicleCountry = vehicleCountryHelper.GetVehicleCountryFromName(countryRawValue).CountryEnum;

						// Weight
						string weightRawValue = vehicleAttributes.Single(k => k.Key == "Weight").Value.ToString();
						int weightWithoutUnits = int.Parse(Regex.Match(weightRawValue, @"\d+").Value);
						string weightUnitsAbbreviation = (Regex.Matches(weightRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleWeightUnitHelper vehicleWeightUnit =
							vehicleWeightUnitHelper.GetWeightUnitFromAbbreviation(weightUnitsAbbreviation);

						// Vehicle class
						string typeRawValue = vehicleAttributes.Single(k => k.Key == "Type").Value.ToString();
						GroundVehicleTypeHelper vehicleType = vehicleTypeHelper.GetGroundVehicleTypeFromName(typeRawValue);

						// Rank
						int rankRawValue = int.Parse(vehicleAttributes.Single(k => k.Key == "Rank").Value.ToString());
						int vehicleRank = rankRawValue;

						// Battle rating
						double ratingRawValue = double.Parse(vehicleAttributes.Single(k => k.Key == "Rating").Value.ToString());
						double vehicleBattleRating = ratingRawValue;

						// Engine power
						string enginePowerRawValue = vehicleAttributes.Single(k => k.Key == "Engine power").Value.ToString();
						int enginePowerWithoutUnits = int.Parse(Regex.Match(enginePowerRawValue, @"\d+").Value);
						string enginePowerUnitsAbbreviation =
							(Regex.Matches(enginePowerRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleEnginePowerUnitHelper vehicleEngineUnit =
							vehicleEnginePowerUnitHelper.GetEngineUnitFromAbbreviation(enginePowerUnitsAbbreviation);

						// Max speed
						string maxSpeedRawValue = vehicleAttributes.Single(k => k.Key == "Max speed").Value.ToString();
						double maxSpeedWithoutUnits = double.Parse(Regex.Match(maxSpeedRawValue, @"\d+\.*\d*").Value);
						string maxSpeedUnits = (Regex.Matches(maxSpeedRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleSpeedUnitHelper vehicleSpeedUnit = vehicleSpeedUnitHelper.GetSpeedUnitFromAbbreviation(maxSpeedUnits);

						// Hull armour
						string hullArmourRawValue = vehicleAttributes.Single(k => k.Key == "Hull armour thickness").Value.ToString();
						string vehicleHullArmourThickness = hullArmourRawValue;

						// Superstructure armour
						string superstructureArmourRawValue =
							vehicleAttributes.Single(k => k.Key == "Superstructure armour thickness").Value.ToString();
						string vehicleSuperstructureArmourThickness = superstructureArmourRawValue;

						// Repair time
						string freeRepairTimeRawValue = vehicleAttributes.Single(k => k.Key == "Time for free repair").Value.ToString();
						List<Match> freeRepairTimeList = (Regex.Matches(freeRepairTimeRawValue, @"\d+").Cast<Match>()).ToList();
						int freeRepairTimeHours = int.Parse(freeRepairTimeList.First().Value);
						int freeRepairTimeMinutes = int.Parse(freeRepairTimeList.Last().Value);
						TimeSpan vehicleFreeRepairTime = new TimeSpan(freeRepairTimeHours, freeRepairTimeMinutes, 0);

						// Max repair cost
						string maxRepairCostRawValue = vehicleAttributes.Single(k => k.Key == "Max repair cost*").Value.ToString();
						string maxRepairCostWithoutUnits = Regex.Match(maxRepairCostRawValue, @"\d+").Value;
						string maxRepairCostUnits = (Regex.Matches(maxRepairCostRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						long vehicleMaxRepairCost = long.Parse(maxRepairCostWithoutUnits);
						VehicleCostUnitHelper vehicleRepairCostUnit = vehicleCostUnitHelper.GetCostUnitFromAbbreviation(maxRepairCostUnits);

						// Purchase cost
						string purchaseCostRawValue = vehicleAttributes.Single(k => k.Key == "Cost*").Value.ToString();
						string purchaseCostWithoutUnits = Regex.Match(purchaseCostRawValue, @"\d+").Value;
						string purchaseCostUnits = (Regex.Matches(purchaseCostRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						long vehiclePurchaseCost = long.Parse(purchaseCostWithoutUnits);
						VehicleCostUnitHelper vehiclePurchaseCostUnit =
							vehicleCostUnitHelper.GetCostUnitFromAbbreviation(purchaseCostUnits);

						// Last modified
						HtmlNode lastModifiedSection =
							vehicleWikiPage.DocumentNode.Descendants().SingleOrDefault(x => x.Id == ConfigurationManager.AppSettings["LastModifiedSectionId"]);
						string lastModified = lastModifiedSection?.InnerHtml;

						// Populate objects
						GroundVehicle groundVehicle = new GroundVehicle
						{
							Name = vehicleName,
							Country = vehicleCountry,
							Weight = weightWithoutUnits,
							VehicleType = vehicleType,
							Rank = vehicleRank,
							BattleRating = vehicleBattleRating,
							EnginePower = enginePowerWithoutUnits,
							MaxSpeed = maxSpeedWithoutUnits,
							HullArmourThickness = vehicleHullArmourThickness,
							SuperstructureArmourThickness = vehicleSuperstructureArmourThickness,
							TimeForFreeRepair = vehicleFreeRepairTime,
							MaxRepairCost = vehicleMaxRepairCost,
							PurchaseCost = vehiclePurchaseCost,
							PurchaseCostUnit = vehiclePurchaseCostUnit,
							MaxRepairCostUnit = vehicleRepairCostUnit,
							MaxSpeedUnit = vehicleSpeedUnit,
							WeightUnit = vehicleWeightUnit,
							EnginePowerUnit = vehicleEngineUnit,
							LastModified = lastModified
						};

						// Update the local storage if requested
						if (ConfigurationManager.AppSettings["UpdateLocalJson"] == "True")
						{
							UpdateLocalStorageForOfflineUse(vehicleWikiPage, vehicleName, LocalWikiFileTypeEnum.JSON, groundVehicle);
						}

						if(ConfigurationManager.AppSettings["UpdateLocalHtml"]== "True")
						{
							UpdateLocalStorageForOfflineUse(vehicleWikiPage, vehicleName, LocalWikiFileTypeEnum.HTML, null);
						}

						if (ConfigurationManager.AppSettings["UpdateExcelDocument"] == "True")
						{
							AddGroundVehicleRowToSpreadsheet(groundVehicle, worksheet);
						}

						//WikiEntry entry = new WikiEntry(vehicleName, vehicleWikiEntryFullUrl, VehicleTypeEnum.Ground, vehicleInfo);

						// Add the found information to the master list
						vehicleDetails.Add(vehicleName, groundVehicle);

						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine($"Processed item {indexPosition} of {expectedNumberOfLinks} successfully");
						Console.WriteLine();
						Console.ResetColor();
					}

					indexPosition++;
				}

				if (ConfigurationManager.AppSettings["UpdateExcelDocument"] == "True")
				{
					// Make columns fit content then save the file
					worksheet.Cells["A1:S1"].AutoFitColumns();
					excelPackage.Save();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		/// <summary>
		/// Creates the headers for the Ground Vehicles spreadsheet
		/// </summary>
		/// <param name="worksheet">The worksheet to create the headers in</param>
		private static void CreateGroundVehicleSpreadsheetHeaders(ExcelWorksheet worksheet)
		{
			//Headers
			worksheet.Cells["A1"].Value = "Name";
			worksheet.Cells["B1"].Value = "Country";
			worksheet.Cells["C1"].Value = "Vehicle Type";
			worksheet.Cells["D1"].Value = "Rank";
			worksheet.Cells["E1"].Value = "Battle Rating";
			worksheet.Cells["F1"].Value = "Weight";
			worksheet.Cells["G1"].Value = "Weight Unit";
			worksheet.Cells["H1"].Value = "Engine Power";
			worksheet.Cells["I1"].Value = "Engine Power Unit";
			worksheet.Cells["J1"].Value = "Max Speed";
			worksheet.Cells["K1"].Value = "Max Speed Unit";
			worksheet.Cells["L1"].Value = "Hull Armour Thickness";
			worksheet.Cells["M1"].Value = "Superstructure Armour Thickness";
			worksheet.Cells["N1"].Value = "Time For Free Repair";
			worksheet.Cells["O1"].Value = "Max Repair Cost";
			worksheet.Cells["P1"].Value = "Max Repair Cost Unit";
			worksheet.Cells["Q1"].Value = "Purchase Cost";
			worksheet.Cells["R1"].Value = "Purchase Cost Unit";
			worksheet.Cells["S1"].Value = "Last Modified";

			worksheet.Cells["A1:S1"].Style.Font.Bold = true;
		}

		/// <summary>
		/// Adds an entry for a ground vehicle to a spreadsheet
		/// </summary>
		/// <param name="groundVehicle">The vehicle to pull the information from</param>
		/// <param name="worksheet">The worksheet to add the data to</param>
		private static void AddGroundVehicleRowToSpreadsheet(GroundVehicle groundVehicle, ExcelWorksheet worksheet)
		{
			if (groundVehicle != null)
			{
				// Get the row we are up to
				int indexPosition = worksheet.Dimension.End.Row + 1;

				// Add values
				worksheet.Cells[$"A{indexPosition}"].Value = groundVehicle.Name;
				worksheet.Cells[$"B{indexPosition}"].Value = groundVehicle.Country;
				worksheet.Cells[$"C{indexPosition}"].Value = groundVehicle.VehicleType.Name;
				worksheet.Cells[$"D{indexPosition}"].Value = groundVehicle.Rank;
				worksheet.Cells[$"E{indexPosition}"].Value = groundVehicle.BattleRating;
				worksheet.Cells[$"F{indexPosition}"].Value = groundVehicle.Weight;
				worksheet.Cells[$"G{indexPosition}"].Value = groundVehicle.WeightUnit.Name;
				worksheet.Cells[$"H{indexPosition}"].Value = groundVehicle.EnginePower;
				worksheet.Cells[$"I{indexPosition}"].Value = groundVehicle.EnginePowerUnit.Name;
				worksheet.Cells[$"J{indexPosition}"].Value = groundVehicle.MaxSpeed;
				worksheet.Cells[$"K{indexPosition}"].Value = groundVehicle.MaxSpeedUnit.Name;
				worksheet.Cells[$"L{indexPosition}"].Value = groundVehicle.HullArmourThickness;
				worksheet.Cells[$"M{indexPosition}"].Value = groundVehicle.SuperstructureArmourThickness;
				worksheet.Cells[$"N{indexPosition}"].Value = groundVehicle.TimeForFreeRepair;
				worksheet.Cells[$"O{indexPosition}"].Value = groundVehicle.MaxRepairCost;
				worksheet.Cells[$"P{indexPosition}"].Value = groundVehicle.MaxRepairCostUnit.Name;
				worksheet.Cells[$"Q{indexPosition}"].Value = groundVehicle.PurchaseCost;
				worksheet.Cells[$"R{indexPosition}"].Value = groundVehicle.PurchaseCostUnit.Name;
				worksheet.Cells[$"S{indexPosition}"].Value = groundVehicle.LastModified;
			}
		}

		/// <summary>
		/// Extracts the key, value pairs from the info box and store them
		/// so that they can be used later on to populate vehicle details
		/// </summary>
		/// <param name="vehicleAttributes">The dictionary to store the key, value pairs in</param>
		/// <param name="rows">The table rows to process and extract the key, value pairs from</param>
		private static void GetAttributesFromInfoBox(Dictionary<string, string> vehicleAttributes, HtmlNodeCollection rows)
		{
			// Traverse the info box and pull out all of the attribute title and value pairs
			foreach (HtmlNode row in rows)
			{
				HtmlNodeCollection cells = row.SelectNodes("td");

				string rowTitle = cells.First().SelectNodes("b").Single().InnerText.Trim();
				string rowValue = cells.Last().InnerText.Trim();

				vehicleAttributes.Add(rowTitle, rowValue);

				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine($"{rowTitle}: {rowValue}");

				if (propertyTotals.ContainsKey(rowTitle))
				{
					int currentCount;
					propertyTotals.TryGetValue(rowTitle, out currentCount);

					propertyTotals[rowTitle] = currentCount + 1;
				}
				else
				{
					propertyTotals.Add(rowTitle, 1);
				}
			}
		}

		/// <summary>
		/// Adds/updates files in the LocalWiki folder
		/// </summary>
		/// <param name="vehicleWikiPage">The HTML content of the wiki page</param>
		/// <param name="vehicleName">The vehicle name of the current wiki page</param>
		private static void UpdateLocalStorageForOfflineUse(HtmlDocument vehicleWikiPage, string vehicleName, LocalWikiFileTypeEnum fileType, IVehicle vehicle = null)
		{
			try
			{
				if (fileType == LocalWikiFileTypeEnum.Undefined)
					throw new ArgumentException("The 'fileType' parameter for the 'UpdateLocalStorageForOfflineUse' is required but was not provided.");

				// Build vars that will be used for the local file
				string fileName = RemoveInvalidCharacters(vehicleName.Replace(' ', '_').Replace('/', '-'));
				string folderPath = fileType == LocalWikiFileTypeEnum.HTML ? ConfigurationManager.AppSettings["LocalWikiHtmlPath"] : ConfigurationManager.AppSettings["LocalWikiJsonPath"];
				string filePath = $@"{folderPath}{fileName}.{fileType.ToString().ToLower()}";

				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				// Handle HTML files
				if (fileType == LocalWikiFileTypeEnum.HTML)
				{
					if (!File.Exists(filePath))
					{
						// Add new item
						vehicleWikiPage.Save($"{filePath}", Encoding.UTF8);
						RecordAddFileToLocalWiki(vehicleName, fileName, fileType.ToString());
					}
					else
					{
						string existingFileText = File.ReadAllText(filePath);

						//Create a fake document so we can use helper methods to traverse through the existing file as an HTML document
						HtmlDocument htmlDoc = new HtmlDocument();
						HtmlNode existingHtml = HtmlNode.CreateNode(existingFileText);
						htmlDoc.DocumentNode.AppendChild(existingHtml);

						// Get out the last modified times for comparison
						var newLastModSection = vehicleWikiPage.DocumentNode.Descendants().SingleOrDefault(x => x.Id == ConfigurationManager.AppSettings["LastModifiedSectionId"]);
						var oldLastModSection = existingHtml.OwnerDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == ConfigurationManager.AppSettings["LastModifiedSectionId"]);

						// If both files have a last modified time
						if (newLastModSection != null && oldLastModSection != null)
						{
							// Update the existing one if the times are different
							if (!AreLastModifiedTimesTheSame(oldLastModSection.InnerHtml, newLastModSection.InnerHtml))
							{
								// Update existing item
								vehicleWikiPage.Save($"{filePath}", Encoding.UTF8);
								RecordUpdateFileInLocalWiki(vehicleName, fileName, fileType.ToString());
							}
						}
						// Add the item if the existing one has no last modified time
						else if (oldLastModSection == null)
						{
							// Update existing item
							vehicleWikiPage.Save($"{filePath}", Encoding.UTF8);
							RecordUpdateFileInLocalWiki(vehicleName, fileName, fileType.ToString());
						}
						else
						{
							throw new InvalidOperationException($"Unable to find the '{ConfigurationManager.AppSettings["LastModifiedSectionId"]}' section, information comparision failed. Most likely the ID of the last modified section has changed.");
						}
					}
				}
				// Handle JSON files
				else if (fileType == LocalWikiFileTypeEnum.JSON)
				{
					GroundVehicle groundVehicle = (GroundVehicle)vehicle;
					string vehicleJson = Newtonsoft.Json.JsonConvert.SerializeObject(groundVehicle, Newtonsoft.Json.Formatting.Indented);

					if (!File.Exists(filePath))
					{
						// Add new item
						File.WriteAllText(filePath, vehicleJson);
						RecordAddFileToLocalWiki(vehicleName, fileName, fileType.ToString());
					}
					else
					{
						string existingFileText = File.ReadAllText(filePath);
						GroundVehicle existingVehicle = Newtonsoft.Json.JsonConvert.DeserializeObject<GroundVehicle>(existingFileText);

						// Get out the last modified times for comparison
						string newLastModSection = groundVehicle.LastModified;
						string oldLastModSection = existingVehicle?.LastModified;

						// If both files have a last modified time
						if (newLastModSection != null && oldLastModSection != null)
						{
							if (!AreLastModifiedTimesTheSame(oldLastModSection, newLastModSection))
							{
								// Update existing
								File.WriteAllText(filePath, vehicleJson);
								RecordUpdateFileInLocalWiki(vehicleName, fileName, fileType.ToString());
							}
						}
						// Add the item if the existing one has no last modified time
						else if (oldLastModSection == null)
						{
							// Update existing item
							File.WriteAllText(filePath, vehicleJson);
							RecordUpdateFileInLocalWiki(vehicleName, fileName, fileType.ToString());
						}
						else
						{
							throw new InvalidOperationException($"Unable to find the '{ConfigurationManager.AppSettings["LastModifiedSectionId"]}' section, information comparision failed.");
						}
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		/// <summary>
		/// Returns whether or not the two timestamps from the last modified section for a vehicle match
		/// </summary>
		/// <param name="oldLastModifiedSection">Timestamp for the older file</param>
		/// <param name="newLastModifiedSection">Timestamp for the newer file</param>
		/// <returns>Whether or not the timestamps match</returns>
		private static bool AreLastModifiedTimesTheSame(string oldLastModifiedSection, string newLastModifiedSection)
		{
			return newLastModifiedSection == oldLastModifiedSection;
		}

		/// <summary>
		/// Removed invalid filename characters from the provided string
		/// </summary>
		/// <param name="dirtyString">The string which could potentially have invalid characters in it</param>
		/// <returns>A string which is valid for file system pathing</returns>
		private static string RemoveInvalidCharacters(string dirtyString)
		{
			var invalidChars = Path.GetInvalidFileNameChars();

			return new string(dirtyString
				.Where(x => !invalidChars.Contains(x))
				.ToArray()
			);
		}

		/// <summary>
		/// Records the addition of a file to the local wiki
		/// </summary>
		/// <param name="vehicleName">Vehicle the file is for</param>
		/// <param name="fileName">File name that was added</param>
		/// <param name="fileType">File type that was added</param>
		private static void RecordAddFileToLocalWiki(string vehicleName, string fileName, string fileType)
		{
			// Record addition of new item
			localFileChanges.TryAdd($"{vehicleName}: {fileType}", $"New vehicle '{fileName}' {fileType} file added to local wiki");
			Console.WriteLine($"New vehicle '{fileName}' {fileType} file added to local wiki");
		}

		/// <summary>
		/// Records the update of a file in the local wiki
		/// </summary>
		/// <param name="vehicleName">Vehicle the file is for</param>
		/// <param name="fileName">File name that was updated</param>
		/// <param name="fileType">File type that was updated</param>
		private static void RecordUpdateFileInLocalWiki(string vehicleName, string fileName, string fileType)
		{
			// Record update of existing item
			localFileChanges.TryAdd($"{vehicleName}: {fileType}", $"Vehicle '{fileName}' {fileType} file updated in local wiki");
			Console.WriteLine($"Vehicle '{fileName}' {fileType} file updated in local wiki");
		}
	}
}