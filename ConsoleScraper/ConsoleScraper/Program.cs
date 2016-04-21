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
		TODO: Support AirForces
		TODO: Support running against local files
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

		public static ConsoleManager ConsoleManager;
		public static ExcelLogger ExcelLogger;

		private static string _currentApplicationVersion = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).FileVersion;

		public static bool CreateJsonFiles = true;
		public static bool CreateHtmlFiles = true;
		public static bool CreateExcelFile = true;

		#region Debugging helpers

		public static Stopwatch overallStopwatch = new Stopwatch();
		public static Stopwatch webCrawlerStopwatch = new Stopwatch();
		public static Stopwatch pageHtmlRetrievalStopwatch = new Stopwatch();

		public static Stopwatch processingStopwatch = new Stopwatch();
		public static Dictionary<string, int> propertyTotals = new Dictionary<string, int>();

		#endregion

		static void Main(string[] args)
		{
			ConsoleManager = new ConsoleManager();
			ExcelLogger = new ExcelLogger();

			try
			{
				overallStopwatch.Start();

				ConsoleManager.WriteLineInColour(ConsoleColor.Green, $"War Thunder Wiki Scraper v{_currentApplicationVersion}");
				ConsoleManager.WriteHorizontalSeparator();
				ConsoleManager.WritePaddedText("Blurb goes here...");
				ConsoleManager.WriteLineInColour(ConsoleColor.Yellow, "Press ENTER to begin.");
				ConsoleManager.WaitUntilKeyIsPressed(ConsoleKey.Enter);
				ConsoleManager.WriteBlankLine();

				HtmlWeb webGet = new HtmlWeb();

				// Load Wiki Home page
				HtmlDocument groundForcesWikiHomePage = webGet.Load(ConfigurationManager.AppSettings["GroundForcesWikiUrl"]);

				// Fail fast if there are errors
				if (groundForcesWikiHomePage.ParseErrors != null && groundForcesWikiHomePage.ParseErrors.Any())
				{
					ConsoleManager.WriteLineInColour(ConsoleColor.Red, "The following errors were encountered:", false);
					ConsoleManager.WriteBlankLine();

					foreach (HtmlParseError error in groundForcesWikiHomePage.ParseErrors)
					{
						ConsoleManager.WriteTextLine(error.Reason);
					}

					ConsoleManager.ResetConsoleTextColour();
				}
				else
				{
					// Setup initial vars
					int totalNumberOfLinksBasedOnPageText = 0;
					int totalNumberOfLinksBasedOnDomTraversal = 0;
					List<HtmlNode> vehicleWikiEntryLinks = new List<HtmlNode>();

					webCrawlerStopwatch.Start();

					// This is outside of the method because of the recursive call and we don't want the user having to press enter more than once
					ConsoleManager.WriteLineInColour(ConsoleColor.Yellow, "Press ENTER to begin searching for links to vehicle pages.");
					ConsoleManager.WaitUntilKeyIsPressed(ConsoleKey.Enter);

					GetLinksToVehiclePages(vehicleWikiEntryLinks, groundForcesWikiHomePage, out totalNumberOfLinksBasedOnPageText, out totalNumberOfLinksBasedOnDomTraversal);

					webCrawlerStopwatch.Stop();

					// Setup thread-safe collections for processing
					ConcurrentDictionary<int, HtmlNode> linksToVehicleWikiPages = new ConcurrentDictionary<int, HtmlNode>();

					// Populate the full list of links we need to traverse
					for (int i = 0; i < vehicleWikiEntryLinks.Count(); i++)
					{
						HtmlNode linkNode = vehicleWikiEntryLinks[i];
						linksToVehicleWikiPages.TryAdd(i, linkNode);
					}

					pageHtmlRetrievalStopwatch.Start();

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

					ConsoleManager.WritePaddedText("Finished extracting html documents from vehicle pages.");

					pageHtmlRetrievalStopwatch.Stop();

					ConsoleManager.WriteHorizontalSeparator();
					ConsoleManager.WriteLineInColour(ConsoleColor.Yellow, "Would you like to create JSON files for each vehicle locally? Enter Y [default] or N.");
					
					CreateJsonFiles = ConsoleManager.IsPressedKeyExpectedKey(ConsoleKey.Y);
					ConsoleManager.WriteLineInColour(ConsoleColor.Green, $"Will{(CreateJsonFiles ? " " : " not ")}create JSON files.");

					ConsoleManager.WriteBlankLine();
					ConsoleManager.WriteLineInColour(ConsoleColor.Yellow, "Would you like to create HTML files for each vehicle locally? Enter Y [default] or N.");

					CreateHtmlFiles = ConsoleManager.IsPressedKeyExpectedKey(ConsoleKey.Y);
					ConsoleManager.WriteLineInColour(ConsoleColor.Green, $"Will{(CreateHtmlFiles ? " " : " not ")}create HTML files.");

					ConsoleManager.WriteBlankLine();
					ConsoleManager.WriteLineInColour(ConsoleColor.Yellow, "Would you like to create an Excel file with all of the vehicle data? Enter Y [default] or N.");

					CreateExcelFile = ConsoleManager.IsPressedKeyExpectedKey(ConsoleKey.Y);
					ConsoleManager.WriteLineInColour(ConsoleColor.Green, $"Will{(CreateExcelFile ? " " : "not")}create Excel file.");
					ConsoleManager.WriteBlankLine();

					int indexPosition = 1;

					// Extract information from the pages we've traversed
					ProcessWikiHtmlFiles(indexPosition, totalNumberOfLinksBasedOnPageText, vehicleWikiEntryLinks);

					ConsoleManager.WriteBlankLine();
					ConsoleManager.WriteLineInColour(ConsoleColor.Green, $"Finished processing html files for vehicle data{(CreateExcelFile || CreateHtmlFiles || CreateJsonFiles ? " and writing local changes." : ".")}");

					// Write out local file changes
					if (localFileChanges.Any())
					{
						ConsoleManager.WriteBlankLine();
						ConsoleManager.WriteTextLine("The following changes were made to the local wiki files: ");

						Dictionary<string, string> orderedLocalFileChanges = localFileChanges.OrderBy(x => x.Key).ToDictionary(d => d.Key, d => d.Value);

						string localChangesFilePath = $"{ConfigurationManager.AppSettings["LocalWikiRootPath"].ToString()}Changes.txt";

						using (StreamWriter streamWriter = File.CreateText(localChangesFilePath))
						{
							foreach (string change in orderedLocalFileChanges.Values)
							{
								ConsoleManager.WriteTextLine(change);
								streamWriter.WriteLine(change);
							}
						}
					}

					ConsoleManager.WriteHorizontalSeparator();

					// Write out errors
					if (errorList.Any())
					{
						ConsoleManager.WriteLineInColour(ConsoleColor.Red, $"The following error{(errorList.Count() > 1 ? "s were" : "was")} encountered:", false);

						string errorFilePath = $"{ConfigurationManager.AppSettings["LocalWikiRootPath"].ToString()}Errors.txt";

						using (StreamWriter streamWriter = File.CreateText(errorFilePath))
						{
							foreach (string error in errorList)
							{
								ConsoleManager.WriteTextLine(error);
								streamWriter.WriteLine(error);
							}
						}

						ConsoleManager.ResetConsoleTextColour();
					}

					ConsoleManager.WriteHorizontalSeparator();

					overallStopwatch.Stop();

					// Write out summary
					TimeSpan timeSpan = overallStopwatch.Elapsed;
					ConsoleManager.WriteTextLine($"Completed in {timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}");
					ConsoleManager.WriteTextLine($"Expected total: {totalNumberOfLinksBasedOnPageText}, Actual total: {totalNumberOfLinksBasedOnDomTraversal}");
					ConsoleManager.WriteTextLine($"Vehicle objects created: {vehicleDetails.Count()} (should be Actual - Errors)");
				}

				// Wait until the user hits 'Esc' to terminate the application
				ConsoleManager.WriteBlankLine();
				ConsoleManager.WriteTextLine("Press ESC to exit...");

				ConsoleManager.WaitUntilKeyIsPressed(ConsoleKey.Escape);
			}
			catch (Exception ex)
			{
				ConsoleManager.WriteLineInColour(ConsoleColor.Red, $"The following exception was encounted: {ex.Message}", false);
				ConsoleManager.WritePaddedText($"Exception details: {ex.StackTrace}");
				ConsoleManager.ResetConsoleTextColour();
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

			if (nextPageLink != null)
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
			else
			{
				ConsoleManager.WriteBlankLine();
				ConsoleManager.WriteLineInColour(ConsoleColor.Green, "Finished retrieving links to vehicle pages.");
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

				ConsoleManager.WriteBlankLine();
				ConsoleManager.WriteTextLine($"Processing... {vehicleName}");
				ConsoleManager.WriteTextLine($"Vehicle: {vehicleName}, Url: {vehicleWikiEntryFullUrl}");

				// Visit page and extract data
				HtmlWeb vehicleWebGet = new HtmlWeb();
				HtmlDocument vehicleWikiPage = vehicleWebGet.Load(vehicleWikiEntryFullUrl);

				// Add page to new dictionary used to extract further data
				vehicleWikiPagesContent.TryAdd(vehicleName, vehicleWikiPage);

				ConsoleManager.WriteTextLine(vehicleWikiPagesContent.Count().ToString());
			}
		}

		/// <summary>
		/// Loops through all of the vehicle wiki links that have been provided, attempts to parse the parts that we are interested in -
		/// the vehicle details table, creates an object from that, then stores the data locally if a flag is set
		/// </summary>
		/// <param name="indexPosition">The current index we are up to processing - used for error messages</param>
		/// <param name="expectedNumberOfLinks">The expected number of links to process</param>
		/// <paran name="vehicleWikiEntryLinks">List that holds the anchor nodes of the relative urls to each vehicle page</paran>
		private static void ProcessWikiHtmlFiles(int indexPosition, int expectedNumberOfLinks, List<HtmlNode> vehicleWikiEntryLinks)
		{
			try
			{
				ConsoleManager.WriteLineInColour(ConsoleColor.Yellow, "Press ENTER to begin extracting data from the vehicle pages.");
				ConsoleManager.WaitUntilKeyIsPressed(ConsoleKey.Enter);

				processingStopwatch.Start();

				foreach (string vehicleWikiPageLinkTitle in vehicleWikiPagesContent.Keys)
				{
					// Page to traverse
					HtmlDocument vehicleWikiPage = vehicleWikiPagesContent.Single(x => x.Key == vehicleWikiPageLinkTitle).Value;
					// Get the header that holds the page title | document.getElementsByClassName('firstHeading')[0].firstChild.innerText
					HtmlNode pageTitle = vehicleWikiPage.DocumentNode.Descendants().Single(d => d.Id == "firstHeading").FirstChild;
					// Get the div that holds all of the content under the title section | document.getElementById('bodyContent')
					HtmlNode wikiBody = vehicleWikiPage.DocumentNode.Descendants().Single(d => d.Id == "bodyContent");
					// Get the div that holds the content on the RHS of the page where the information table is | document.getElementById('bodyContent').getElementsByClassName('right-area')
					HtmlNode rightHandContent = wikiBody.Descendants("div").SingleOrDefault(d => d.Attributes["class"] != null && d.Attributes["class"].Value.Contains("right-area"));

					// Get the able that holds all of the vehicle information | document.getElementsByClassName('flight-parameters')[0]
					HtmlNode infoBox = rightHandContent != null
						? rightHandContent.Descendants("table").SingleOrDefault(d => d.Attributes["class"].Value.Contains("flight-parameters"))
						: null;

					// Name
					string vehicleName = RemoveInvalidCharacters(System.Net.WebUtility.HtmlDecode(vehicleWikiPageLinkTitle));

					// Link
					HtmlNode urlNode = vehicleWikiEntryLinks.SingleOrDefault(v => v.InnerText.Equals(vehicleName));
					string relativeUrl = urlNode != null
						? urlNode.Attributes["href"].Value.ToString()
						: "";
					string vehicleWikiEntryFullUrl = new Uri(new Uri(ConfigurationManager.AppSettings["BaseWikiUrl"]), relativeUrl).ToString();

					// Fail fast and create error if there is no info box
					if (infoBox == null)
					{
						ConsoleManager.WriteLineInColour(ConsoleColor.Red, $"Error processing item {indexPosition} of {expectedNumberOfLinks}", false);
						ConsoleManager.WriteBlankLine();

						errorList.Add($"No Information found for '{vehicleName}' - {vehicleWikiEntryFullUrl}");

						ConsoleManager.ResetConsoleTextColour();
						indexPosition++;
						continue;
					}
					else
					{
						// Setup local vars
						Dictionary<string, string> vehicleAttributes = new Dictionary<string, string>();
						HtmlNodeCollection rows = infoBox.SelectNodes("tr");

						ConsoleManager.WriteTextLine($"The following values were found for {vehicleName}");

						GetAttributesFromInfoBox(vehicleAttributes, rows);

						ConsoleManager.ResetConsoleTextColour();

						// Country
						string countryRawValue = vehicleAttributes.Single(k => k.Key == "Country").Value.ToString();
						CountryEnum vehicleCountry = vehicleCountryHelper.GetVehicleCountryFromName(countryRawValue).CountryEnum;

						// Weight
						string weightRawValue = vehicleAttributes.Single(k => k.Key == "Weight").Value.ToString();
						int weightWithoutUnits = int.Parse(Regex.Match(weightRawValue, @"\d+").Value);
						string weightUnitsAbbreviation = (Regex.Matches(weightRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleWeightUnitHelper vehicleWeightUnit = vehicleWeightUnitHelper.GetWeightUnitFromAbbreviation(weightUnitsAbbreviation);

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
						string enginePowerUnitsAbbreviation = (Regex.Matches(enginePowerRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleEnginePowerUnitHelper vehicleEngineUnit = vehicleEnginePowerUnitHelper.GetEngineUnitFromAbbreviation(enginePowerUnitsAbbreviation);

						// Max speed
						string maxSpeedRawValue = vehicleAttributes.Single(k => k.Key == "Max speed").Value.ToString();
						double maxSpeedWithoutUnits = double.Parse(Regex.Match(maxSpeedRawValue, @"\d+\.*\d*").Value);
						string maxSpeedUnits = (Regex.Matches(maxSpeedRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleSpeedUnitHelper vehicleSpeedUnit = vehicleSpeedUnitHelper.GetSpeedUnitFromAbbreviation(maxSpeedUnits);

						// Hull armour
						string hullArmourRawValue = vehicleAttributes.Single(k => k.Key == "Hull armour thickness").Value.ToString();
						string vehicleHullArmourThickness = hullArmourRawValue;

						// Superstructure armour
						string superstructureArmourRawValue = vehicleAttributes.Single(k => k.Key == "Superstructure armour thickness").Value.ToString();
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
						VehicleCostUnitHelper vehiclePurchaseCostUnit = vehicleCostUnitHelper.GetCostUnitFromAbbreviation(purchaseCostUnits);

						// Last modified
						HtmlNode lastModifiedSection = vehicleWikiPage.DocumentNode.Descendants().SingleOrDefault(x => x.Id == ConfigurationManager.AppSettings["LastModifiedSectionId"]);
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
						if (CreateJsonFiles)
						{
							UpdateLocalStorageForOfflineUse(vehicleWikiPage, vehicleName, LocalWikiFileTypeEnum.JSON, groundVehicle);
						}

						if (CreateHtmlFiles)
						{
							UpdateLocalStorageForOfflineUse(vehicleWikiPage, vehicleName, LocalWikiFileTypeEnum.HTML, null);
						}

						//WikiEntry entry = new WikiEntry(vehicleName, vehicleWikiEntryFullUrl, VehicleTypeEnum.Ground, vehicleInfo);

						// Add the found information to the master list
						vehicleDetails.Add(vehicleName, groundVehicle);

						ConsoleManager.WriteLineInColour(ConsoleColor.Green, $"Processed item {indexPosition} of {expectedNumberOfLinks} successfully");
						ConsoleManager.WriteBlankLine();
					}

					indexPosition++;
				}

				if (CreateExcelFile)
				{
					ExcelLogger.CreateExcelFile(vehicleDetails);
				}

				processingStopwatch.Stop();
			}
			catch (Exception ex)
			{
				ConsoleManager.WriteException(ex.Message);
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

				ConsoleManager.WriteLineInColour(ConsoleColor.DarkGreen, $"{rowTitle}: {rowValue}");

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
							string noLastModifiedSectionExceptionMessage = $"Unable to find the '{ConfigurationManager.AppSettings["LastModifiedSectionId"]}' section, information comparision failed.";

							ConsoleManager.WriteException(noLastModifiedSectionExceptionMessage);
							throw new InvalidOperationException(noLastModifiedSectionExceptionMessage);
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
							string noLastModifiedSectionExceptionMessage = $"Unable to find the '{ConfigurationManager.AppSettings["LastModifiedSectionId"]}' section, information comparision failed.";

							ConsoleManager.WriteException(noLastModifiedSectionExceptionMessage);
							throw new InvalidOperationException(noLastModifiedSectionExceptionMessage);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ConsoleManager.WriteException(ex.Message);
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
			ConsoleManager.WriteTextLine($"New vehicle '{fileName}' {fileType} file added to local wiki");
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
			ConsoleManager.WriteTextLine($"Vehicle '{fileName}' {fileType} file updated in local wiki");
		}

		
	}
}