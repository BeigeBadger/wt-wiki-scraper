using ConsoleScraper.Enums;
using ConsoleScraper.Logging.Interfaces;
using ConsoleScraper.Models;
using ConsoleScraper.Util.Interfaces;
using ConsoleScraper.Util.ParsingHelpers;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleScraper.Util.Processors
{
	public class DataProcessor : IDataProcessor
	{
		private readonly IConsoleManager _consoleManager;
		private readonly IStringHelper _stringHelper;
		private readonly IWebCrawler _webCrawler;
		private readonly IExcelLogger _excelLogger;
		private readonly ILogger _logger;

		#region Debugging helpers

		private readonly Stopwatch _webCrawlerStopwatch = new Stopwatch();
		private readonly Stopwatch _pageHtmlRetrievalStopwatch = new Stopwatch();
		private readonly Stopwatch _processingStopwatch = new Stopwatch();

		#endregion Debugging helpers

		private bool _createJsonFiles = true;
		private bool _createHtmlFiles = true;
		private bool _createExcelFile = true;

		/// <summary>
		/// Populated with the vehicle name as the key and the HTML content of the page as the value
		/// </summary>
		private readonly ConcurrentDictionary<string, HtmlDocument> _vehicleWikiPagesContent = new ConcurrentDictionary<string, HtmlDocument>();

		/// <summary>
		/// Keeps track of changes made to local files, vehicle name as the key, and the action performed as the value
		/// </summary>
		private readonly ConcurrentDictionary<string, string> _localFileChanges = new ConcurrentDictionary<string, string>();

		/// <summary>
		/// Populated with the vehicle name and vehicle objects
		/// </summary>
		private readonly Dictionary<string, GroundVehicle> _vehicleDetails = new Dictionary<string, GroundVehicle>();

		/// <summary>
		/// Holds all of the errors that were encountered during processing
		/// </summary>
		private readonly List<string> _errorsList = new List<string>();

		// Helper objects
		private readonly VehicleCostUnitHelper _vehicleCostUnitHelper = new VehicleCostUnitHelper();

		private readonly VehicleSpeedUnitHelper _vehicleSpeedUnitHelper = new VehicleSpeedUnitHelper();
		private readonly VehicleWeightUnitHelper _vehicleWeightUnitHelper = new VehicleWeightUnitHelper();
		private readonly VehicleCountryHelper _vehicleCountryHelper = new VehicleCountryHelper();
		private readonly GroundVehicleTypeHelper _vehicleTypeHelper = new GroundVehicleTypeHelper();
		private readonly VehicleEnginePowerUnitHelper _vehicleEnginePowerUnitHelper = new VehicleEnginePowerUnitHelper();

		public DataProcessor(IConsoleManager consoleManager, IStringHelper stringHelper, IWebCrawler webCrawler, IExcelLogger excelLogger, ILogger logger)
		{
			_consoleManager = consoleManager;
			_stringHelper = stringHelper;
			_webCrawler = webCrawler;
			_excelLogger = excelLogger;
			_logger = logger;
		}

		// TODO: VehicleDetails will have to be changed to take an IVehicle as the value data type
		// TODO: Pass in Enum of vehicle type to use for the processing call
		// TODO: Fix message on bool createFile parameters - maybe refactor
		public void CrawlWikiSectionPagesForData(HtmlDocument wikiHomePage)
		{
			bool parseErrorsEncountered = _webCrawler.DoesTheDocumentContainParseErrors(wikiHomePage);

			if (parseErrorsEncountered)
			{
				_consoleManager.HandleHtmlParseErrors(wikiHomePage);
			}
			else
			{
				// Setup initial vars
				List<HtmlNode> vehicleWikiEntryLinks = new List<HtmlNode>();

				_webCrawlerStopwatch.Start();

				// This is outside of the method because of the recursive call and we don't want the user having to press enter more than once
				_consoleManager.WriteInputInstructionsAndAwaitUserInput(ConsoleColor.Yellow, ConsoleKey.Enter, "Press ENTER to begin searching for links to vehicle pages.");

				Dictionary<string, int> linksFound = _webCrawler.GetLinksToVehiclePages(vehicleWikiEntryLinks, wikiHomePage);
				int totalNumberOfLinksBasedOnPageText = linksFound.Single(l => l.Key.Equals("TotalNumberOfLinksBasedOnPageText")).Value;
				int totalNumberOfLinksFoundViaDomTraversal = linksFound.Single(l => l.Key.Equals("TotalNumberOfLinksFoundViaDomTraversal")).Value;

				_webCrawlerStopwatch.Stop();

				// Setup thread-safe collections for processing
				ConcurrentDictionary<int, HtmlNode> linksToVehicleWikiPages = new ConcurrentDictionary<int, HtmlNode>();

				// Populate the full list of links we need to traverse
				for (int i = 0; i < vehicleWikiEntryLinks.Count; i++)
				{
					HtmlNode linkNode = vehicleWikiEntryLinks[i];
					linksToVehicleWikiPages.TryAdd(i, linkNode);
				}

				_pageHtmlRetrievalStopwatch.Start();

				_consoleManager.WriteLineInColourPreceededByBlankLine(ConsoleColor.Green, "Attempting to extract the vehicle pages as HTML documents.");

				// Crawl the pages concurrently
				Task[] webCrawlerTasks = {
					// Going from 2 to 4 tasks halves the processing time, after 4 tasks the performance gain is negligible
					Task.Factory.StartNew(() => _webCrawler.GetPageHtml(linksToVehicleWikiPages, _vehicleWikiPagesContent)),
					Task.Factory.StartNew(() => _webCrawler.GetPageHtml(linksToVehicleWikiPages, _vehicleWikiPagesContent)),
					Task.Factory.StartNew(() => _webCrawler.GetPageHtml(linksToVehicleWikiPages, _vehicleWikiPagesContent)),
					Task.Factory.StartNew(() => _webCrawler.GetPageHtml(linksToVehicleWikiPages, _vehicleWikiPagesContent))
				};

				// Wait until we have crawled all of the pages
				Task.WaitAll(webCrawlerTasks);

				_consoleManager.WriteLineInColourPreceededByBlankLine(ConsoleColor.Green, "Finished extracting the vehicle pages as HTML documents.");

				_pageHtmlRetrievalStopwatch.Stop();

				_consoleManager.WriteHorizontalSeparator();

				_consoleManager.HandleCreateFileTypePrompts(out _createJsonFiles, out _createHtmlFiles, out _createExcelFile);

				int indexPosition = 1;

				_processingStopwatch.Start();

				// Extract information from the pages we've traversed
				ProcessGroundForcesWikiHtmlFiles(_vehicleWikiPagesContent, _localFileChanges, _vehicleDetails, vehicleWikiEntryLinks, _errorsList, indexPosition, totalNumberOfLinksBasedOnPageText, _createJsonFiles, _createHtmlFiles, _createExcelFile);

				_processingStopwatch.Stop();

				_consoleManager.WriteLineInColourPreceededByBlankLine(ConsoleColor.Green, $"Finished processing html files for vehicle data{(_createExcelFile || _createHtmlFiles || _createJsonFiles ? " and writing local changes." : ".")}");

				if (_localFileChanges.Any())
				{
					_logger.HandleLocalFileChanges(_localFileChanges);
				}

				_consoleManager.WriteHorizontalSeparator();

				if (_errorsList.Any())
				{
					_logger.HandleProcessingErrors(_errorsList);

					string errorFilePath = Path.GetFullPath($"{ConfigurationManager.AppSettings["LocalWikiRootPath"]}Errors.txt");

					_consoleManager.WritePaddedText($"An error log has stored in {errorFilePath}.");
				}

				_consoleManager.WriteHorizontalSeparator();
				_consoleManager.WriteProcessingSummary(totalNumberOfLinksBasedOnPageText, totalNumberOfLinksFoundViaDomTraversal, _vehicleDetails.Count, _errorsList.Count);
			}
		}

		public void ProcessGroundForcesWikiHtmlFiles(ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent, ConcurrentDictionary<string, string> localFileChanges, Dictionary<string, GroundVehicle> vehicleDetails, List<HtmlNode> vehicleWikiEntryLinks, List<string> errorsList, int indexPosition, int expectedNumberOfLinks, bool createJsonFiles, bool createHtmlFiles, bool createExcelFile)
		{
			try
			{
				_consoleManager.WriteLineInColour(ConsoleColor.Yellow, "Press ENTER to begin extracting data from the vehicle pages.");
				_consoleManager.WaitUntilKeyIsPressed(ConsoleKey.Enter);

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
					HtmlNode infoBox = rightHandContent?.Descendants("table").SingleOrDefault(d => d.Attributes["class"].Value.Contains("flight-parameters"));

					// Name
					string vehicleName = _stringHelper.RemoveInvalidCharacters(WebUtility.HtmlDecode(vehicleWikiPageLinkTitle));

					// Link
					HtmlNode urlNode = vehicleWikiEntryLinks.SingleOrDefault(v => v.InnerText.Equals(vehicleName));
					string relativeUrl = urlNode?.Attributes["href"].Value ?? "";
					string vehicleWikiEntryFullUrl = new Uri(new Uri(ConfigurationManager.AppSettings["BaseWikiUrl"]), relativeUrl).ToString();

					// Fail fast and create error if there is no info box
					if (infoBox == null)
					{
						_consoleManager.WriteLineInColour(ConsoleColor.Red, $"Error processing item {indexPosition} of {expectedNumberOfLinks}", false);
						_consoleManager.WriteBlankLine();

						errorsList.Add($"No Information found for '{vehicleName}' - {vehicleWikiEntryFullUrl}");

						_consoleManager.ResetConsoleTextColour();
						indexPosition++;
						continue;
					}
					else
					{
						// Setup local vars
						Dictionary<string, string> vehicleAttributes = new Dictionary<string, string>();
						HtmlNodeCollection rows = infoBox.SelectNodes("tr");

						_consoleManager.WriteTextLine($"The following values were found for {vehicleName}");

						_webCrawler.GetAttributesFromInfoBox(vehicleAttributes, rows);

						_consoleManager.ResetConsoleTextColour();

						// Country
						string countryRawValue = vehicleAttributes.Single(k => k.Key == "Country").Value;
						CountryEnum vehicleCountry = _vehicleCountryHelper.GetVehicleCountryFromName(countryRawValue).CountryEnum;

						// Weight
						string weightRawValue = vehicleAttributes.Single(k => k.Key == "Weight").Value;
						int weightWithoutUnits = Int32.Parse(Regex.Match(weightRawValue, @"\d+").Value);
						string weightUnitsAbbreviation = (Regex.Matches(weightRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleWeightUnitHelper vehicleWeightUnit = _vehicleWeightUnitHelper.GetWeightUnitFromAbbreviation(weightUnitsAbbreviation);

						// Vehicle class
						string typeRawValue = vehicleAttributes.Single(k => k.Key == "Type").Value;
						GroundVehicleTypeHelper vehicleType = _vehicleTypeHelper.GetGroundVehicleTypeFromName(typeRawValue);

						// Rank
						int rankRawValue = Int32.Parse(vehicleAttributes.Single(k => k.Key == "Rank").Value);
						int vehicleRank = rankRawValue;

						// Battle rating
						double ratingRawValue = Double.Parse(vehicleAttributes.Single(k => k.Key == "Rating").Value);
						double vehicleBattleRating = ratingRawValue;

						// Engine power
						string enginePowerRawValue = vehicleAttributes.Single(k => k.Key == "Engine power").Value;
						int enginePowerWithoutUnits = Int32.Parse(Regex.Match(enginePowerRawValue, @"\d+").Value);
						string enginePowerUnitsAbbreviation = (Regex.Matches(enginePowerRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleEnginePowerUnitHelper vehicleEngineUnit = _vehicleEnginePowerUnitHelper.GetEngineUnitFromAbbreviation(enginePowerUnitsAbbreviation);

						// Max speed
						string maxSpeedRawValue = vehicleAttributes.Single(k => k.Key == "Max speed").Value;
						double maxSpeedWithoutUnits = Double.Parse(Regex.Match(maxSpeedRawValue, @"\d+\.*\d*").Value);
						string maxSpeedUnits = (Regex.Matches(maxSpeedRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleSpeedUnitHelper vehicleSpeedUnit = _vehicleSpeedUnitHelper.GetSpeedUnitFromAbbreviation(maxSpeedUnits);

						// Hull armour
						string hullArmourRawValue = vehicleAttributes.Single(k => k.Key == "Hull armour thickness").Value;
						string vehicleHullArmourThickness = hullArmourRawValue;

						// Superstructure armour
						string superstructureArmourRawValue = vehicleAttributes.Single(k => k.Key == "Superstructure armour thickness").Value;
						string vehicleSuperstructureArmourThickness = superstructureArmourRawValue;

						// Repair time
						string freeRepairTimeRawValue = vehicleAttributes.Single(k => k.Key == "Time for free repair").Value;
						List<Match> freeRepairTimeList = (Regex.Matches(freeRepairTimeRawValue, @"\d+").Cast<Match>()).ToList();
						int freeRepairTimeHours = Int32.Parse(freeRepairTimeList.First().Value);
						int freeRepairTimeMinutes = Int32.Parse(freeRepairTimeList.Last().Value);
						TimeSpan vehicleFreeRepairTime = new TimeSpan(freeRepairTimeHours, freeRepairTimeMinutes, 0);

						// Max repair cost
						string maxRepairCostRawValue = vehicleAttributes.Single(k => k.Key == "Max repair cost*").Value;
						string maxRepairCostWithoutUnits = Regex.Match(maxRepairCostRawValue, @"\d+").Value;
						string maxRepairCostUnits = (Regex.Matches(maxRepairCostRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						long vehicleMaxRepairCost = Int64.Parse(maxRepairCostWithoutUnits);
						VehicleCostUnitHelper vehicleRepairCostUnit = _vehicleCostUnitHelper.GetCostUnitFromAbbreviation(maxRepairCostUnits);

						// Purchase cost
						string purchaseCostRawValue = vehicleAttributes.Single(k => k.Key == "Cost*").Value;
						string purchaseCostWithoutUnits = Regex.Match(purchaseCostRawValue, @"\d+").Value;
						string purchaseCostUnits = (Regex.Matches(purchaseCostRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						long vehiclePurchaseCost = Int64.Parse(purchaseCostWithoutUnits);
						VehicleCostUnitHelper vehiclePurchaseCostUnit = _vehicleCostUnitHelper.GetCostUnitFromAbbreviation(purchaseCostUnits);

						// Last modified
						HtmlNode lastModifiedSection = vehicleWikiPage.DocumentNode.Descendants().SingleOrDefault(x => x.Id == ConfigurationManager.AppSettings["LastModifiedSectionId"]);
						string lastModified = lastModifiedSection?.InnerHtml;

						// Populate objects
						GroundVehicle groundVehicle = new GroundVehicle
						{
							Name = vehicleName,
							Country = vehicleCountry,
							Weight = weightWithoutUnits,
							VehicleType = (VehicleTypeEnum)vehicleType.Id,
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
						if (createJsonFiles)
						{
							_logger.UpdateLocalStorageForOfflineUse(localFileChanges, vehicleWikiPage, vehicleName, LocalWikiFileTypeEnum.Json, groundVehicle);
						}

						if (createHtmlFiles)
						{
							_logger.UpdateLocalStorageForOfflineUse(localFileChanges, vehicleWikiPage, vehicleName, LocalWikiFileTypeEnum.Html);
						}

						//WikiEntry entry = new WikiEntry(vehicleName, vehicleWikiEntryFullUrl, VehicleTypeEnum.Ground, vehicleInfo);

						// Add the found information to the master list
						vehicleDetails.Add(vehicleName, groundVehicle);

						_consoleManager.WriteLineInColour(ConsoleColor.Green, $"Processed item {indexPosition} of {expectedNumberOfLinks} successfully");
						_consoleManager.WriteBlankLine();
					}

					indexPosition++;
				}

				if (createExcelFile)
				{
					_excelLogger.CreateExcelFile(vehicleDetails);
				}
			}
			catch (Exception ex)
			{
				_consoleManager.WriteException(ex.Message);
			}
		}
	}
}