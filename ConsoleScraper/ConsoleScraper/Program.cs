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
using System.Configuration;
using ConsoleScraper.Enums;

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
		public static FilePerVehicleLogger FilePerVehicleLogger;
		public static HtmlLogger HtmlLogger;
		public static JsonLogger JsonLogger;
		public static WebCrawler WebCrawler;
		public static StringHelper StringHelper;

		private static string _currentApplicationVersion = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).FileVersion;

		public static bool CreateJsonFiles = true;
		public static bool CreateHtmlFiles = true;
		public static bool CreateExcelFile = true;

		#region Debugging helpers

		public static Stopwatch overallStopwatch = new Stopwatch();
		public static Stopwatch webCrawlerStopwatch = new Stopwatch();
		public static Stopwatch pageHtmlRetrievalStopwatch = new Stopwatch();

		public static Stopwatch processingStopwatch = new Stopwatch();

		#endregion

		static void Main(string[] args)
		{
			ConsoleManager = new ConsoleManager();
			ExcelLogger = new ExcelLogger();
			FilePerVehicleLogger = new FilePerVehicleLogger(ConsoleManager);

			HtmlLogger = new HtmlLogger(FilePerVehicleLogger, FilePerVehicleLogger.ConsoleManager);
			JsonLogger = new JsonLogger(FilePerVehicleLogger, FilePerVehicleLogger.ConsoleManager);

			WebCrawler = new WebCrawler(ConsoleManager);

			StringHelper = new StringHelper();

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
					List<HtmlNode> vehicleWikiEntryLinks = new List<HtmlNode>();

					webCrawlerStopwatch.Start();

					// This is outside of the method because of the recursive call and we don't want the user having to press enter more than once
					ConsoleManager.WriteLineInColour(ConsoleColor.Yellow, "Press ENTER to begin searching for links to vehicle pages.");
					ConsoleManager.WaitUntilKeyIsPressed(ConsoleKey.Enter);

					Dictionary<string, int> linksFound = WebCrawler.GetLinksToVehiclePages(vehicleWikiEntryLinks, groundForcesWikiHomePage);
					int totalNumberOfLinksBasedOnPageText = linksFound.Where(l => l.Key.Equals("TotalNumberOfLinksBasedOnPageText")).Single().Value;
					int totalNumberOfLinksFoundViaDomTraversal = linksFound.Where(l => l.Key.Equals("TotalNumberOfLinksFoundViaDomTraversal")).Single().Value;

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
						Task.Factory.StartNew(() => WebCrawler.GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent)),
						Task.Factory.StartNew(() => WebCrawler.GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent)),
						Task.Factory.StartNew(() => WebCrawler.GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent)),
						Task.Factory.StartNew(() => WebCrawler.GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent))
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
					ConsoleManager.WriteTextLine($"Expected total: {totalNumberOfLinksBasedOnPageText}, Actual total: {totalNumberOfLinksFoundViaDomTraversal}");
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
					string vehicleName = StringHelper.RemoveInvalidCharacters(System.Net.WebUtility.HtmlDecode(vehicleWikiPageLinkTitle));

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

						WebCrawler.GetAttributesFromInfoBox(vehicleAttributes, rows);

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
				string fileName = StringHelper.RemoveInvalidCharacters(vehicleName.Replace(' ', '_').Replace('/', '-'));
				string folderPath = fileType == LocalWikiFileTypeEnum.HTML ? ConfigurationManager.AppSettings["LocalWikiHtmlPath"] : ConfigurationManager.AppSettings["LocalWikiJsonPath"];
				string filePath = $@"{folderPath}{fileName}.{fileType.ToString().ToLower()}";

				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				// Handle HTML files
				if (fileType == LocalWikiFileTypeEnum.HTML)
				{
					HtmlLogger.CreateHtmlFile(localFileChanges, vehicleWikiPage, vehicleName, fileName, filePath);
				}
				// Handle JSON files
				else if (fileType == LocalWikiFileTypeEnum.JSON)
				{
					JsonLogger.CreateJsonFile(localFileChanges, vehicleName, vehicle, fileName, filePath);
				}
			}
			catch (Exception ex)
			{
				ConsoleManager.WriteException(ex.Message);
			}
		}
	}
}