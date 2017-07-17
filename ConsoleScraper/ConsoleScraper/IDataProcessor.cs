using ConsoleScraper.Enums;
using ConsoleScraper.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleScraper
{
	public interface IDataProcessor
	{
		/// <summary>
		/// Finds links to all of the wiki pages for a vehicle type,
		/// then goes through and visits each one, extracting the
		/// data into an HtmlDocument for us in the Process method.
		/// </summary>
		/// <param name="wikiHomePage">The document containing the links to the vehicle wiki pages</param>
		/// <param name="vehicleWikiPagesContent">Holds the HtmlDocuments that are retrieved</param>
		/// <param name="localFileChanges">Holds all of the changes to local files</param>
		/// <param name="vehicleDetails">Holds a mapping between the page title and the vehicle object with the extracted data</param>
		/// <param name="errorsList">Holds the errors that were encountered during processing</param>
		/// <param name="overallStopwatch">Debug only: Holds the stopwatch that is used to record the execution time</param>
		/// <param name="createJsonFiles">Holds whether or not to create Json files as an output</param>
		/// <param name="createHtmlFiles">Holds whether or not to create Html files as an output</param>
		/// <param name="createExcelFile">Holds whether or not to create an Excel file as an output</param>
		void CrawlWikiSectionPagesForData(HtmlDocument wikiHomePage, ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent,
			ConcurrentDictionary<string, string> localFileChanges, Dictionary<string, GroundVehicle> vehicleDetails, List<string> errorsList,
			Stopwatch overallStopwatch, bool createJsonFiles, bool createHtmlFiles, bool createExcelFile);

		/// <summary>
		/// Loops through all of the vehicle wiki links that have been provided, attempts to parse the parts that we are interested in -
		/// the vehicle details table, creates an object from that, then stores the data locally if a flag is set
		/// </summary>
		/// <param name="vehicleWikiPagesContent">Populated with the vehicle name as the key and the HTML content of the page as the value</param>
		/// <param name="localFileChanges">Keeps track of changes made to local files, vehicle name as the key, and the action performed as the value</param>
		/// <param name="vehicleDetails">Populated with the vehicle name and vehicle objects</param>
		/// <param name="vehicleWikiEntryLinks">List that holds the anchor nodes of the relative urls to each vehicle page</param>
		/// <param name="errorsList">Holds any errors that occur during execution</param>
		/// <param name="indexPosition">The current index we are up to processing - used for error messages</param>
		/// <param name="expectedNumberOfLinks">The expected number of links to process</param>
		/// <param name="createJsonFiles">Whether or not to output a JSON file per vehicle</param>
		/// <param name="createHtmlFiles">Whether or not to output a HTML file per vehicle</param>
		/// <param name="createExcelFile">Whether or not to output a Excel file per vehicle</param>
		void ProcessGroundForcesWikiHtmlFiles(ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent, ConcurrentDictionary<string, string> localFileChanges,
			Dictionary<string, GroundVehicle> vehicleDetails, List<HtmlNode> vehicleWikiEntryLinks, List<string> errorsList, int indexPosition, int expectedNumberOfLinks,
			bool createJsonFiles, bool createHtmlFiles, bool createExcelFile);
	}

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
		public void CrawlWikiSectionPagesForData(HtmlDocument wikiHomePage, ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent,
			ConcurrentDictionary<string, string> localFileChanges, Dictionary<string, GroundVehicle> vehicleDetails, List<string> errorsList,
			Stopwatch overallStopwatch, bool createJsonFiles, bool createHtmlFiles, bool createExcelFile)
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

				// Crawl the pages concurrently
				Task[] webCrawlerTasks = {
						// Going from 2 to 4 tasks halves the processing time, after 4 tasks the performance gain is negligible
						Task.Factory.StartNew(() => _webCrawler.GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent)),
						Task.Factory.StartNew(() => _webCrawler.GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent)),
						Task.Factory.StartNew(() => _webCrawler.GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent)),
						Task.Factory.StartNew(() => _webCrawler.GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent))
				};

				// Wait until we have crawled all of the pages
				Task.WaitAll(webCrawlerTasks);

				_consoleManager.WritePaddedText("Finished extracting html documents from vehicle pages.");

				_pageHtmlRetrievalStopwatch.Stop();

				_consoleManager.WriteHorizontalSeparator();

				_consoleManager.HandleCreateFileTypePrompts(createJsonFiles, createHtmlFiles, createExcelFile);

				int indexPosition = 1;

				_processingStopwatch.Start();

				// Extract information from the pages we've traversed
				ProcessGroundForcesWikiHtmlFiles(vehicleWikiPagesContent, localFileChanges, vehicleDetails, vehicleWikiEntryLinks, errorsList, indexPosition, totalNumberOfLinksBasedOnPageText, createJsonFiles, createHtmlFiles, createExcelFile);

				_processingStopwatch.Stop();

				_consoleManager.WriteLineInColourPreceededByBlankLine(ConsoleColor.Green, $"Finished processing html files for vehicle data{(createExcelFile || createHtmlFiles || createJsonFiles ? " and writing local changes." : ".")}");

				if (localFileChanges.Any())
				{
					_logger.HandleLocalFileChanges(localFileChanges);
				}

				_consoleManager.WriteHorizontalSeparator();

				if (errorsList.Any())
				{
					_logger.HandleProcessingErrors(errorsList);
				}

				_consoleManager.WriteHorizontalSeparator();

				overallStopwatch.Stop();

				_consoleManager.WriteProcessingSummary(overallStopwatch.Elapsed, totalNumberOfLinksBasedOnPageText, totalNumberOfLinksFoundViaDomTraversal, vehicleDetails.Count);
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
					string vehicleName = _stringHelper.RemoveInvalidCharacters(System.Net.WebUtility.HtmlDecode(vehicleWikiPageLinkTitle));

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
						int weightWithoutUnits = int.Parse(Regex.Match(weightRawValue, @"\d+").Value);
						string weightUnitsAbbreviation = (Regex.Matches(weightRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleWeightUnitHelper vehicleWeightUnit = _vehicleWeightUnitHelper.GetWeightUnitFromAbbreviation(weightUnitsAbbreviation);

						// Vehicle class
						string typeRawValue = vehicleAttributes.Single(k => k.Key == "Type").Value;
						GroundVehicleTypeHelper vehicleType = _vehicleTypeHelper.GetGroundVehicleTypeFromName(typeRawValue);

						// Rank
						int rankRawValue = int.Parse(vehicleAttributes.Single(k => k.Key == "Rank").Value);
						int vehicleRank = rankRawValue;

						// Battle rating
						double ratingRawValue = double.Parse(vehicleAttributes.Single(k => k.Key == "Rating").Value);
						double vehicleBattleRating = ratingRawValue;

						// Engine power
						string enginePowerRawValue = vehicleAttributes.Single(k => k.Key == "Engine power").Value;
						int enginePowerWithoutUnits = int.Parse(Regex.Match(enginePowerRawValue, @"\d+").Value);
						string enginePowerUnitsAbbreviation = (Regex.Matches(enginePowerRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleEnginePowerUnitHelper vehicleEngineUnit = _vehicleEnginePowerUnitHelper.GetEngineUnitFromAbbreviation(enginePowerUnitsAbbreviation);

						// Max speed
						string maxSpeedRawValue = vehicleAttributes.Single(k => k.Key == "Max speed").Value;
						double maxSpeedWithoutUnits = double.Parse(Regex.Match(maxSpeedRawValue, @"\d+\.*\d*").Value);
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
						int freeRepairTimeHours = int.Parse(freeRepairTimeList.First().Value);
						int freeRepairTimeMinutes = int.Parse(freeRepairTimeList.Last().Value);
						TimeSpan vehicleFreeRepairTime = new TimeSpan(freeRepairTimeHours, freeRepairTimeMinutes, 0);

						// Max repair cost
						string maxRepairCostRawValue = vehicleAttributes.Single(k => k.Key == "Max repair cost*").Value;
						string maxRepairCostWithoutUnits = Regex.Match(maxRepairCostRawValue, @"\d+").Value;
						string maxRepairCostUnits = (Regex.Matches(maxRepairCostRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						long vehicleMaxRepairCost = long.Parse(maxRepairCostWithoutUnits);
						VehicleCostUnitHelper vehicleRepairCostUnit = _vehicleCostUnitHelper.GetCostUnitFromAbbreviation(maxRepairCostUnits);

						// Purchase cost
						string purchaseCostRawValue = vehicleAttributes.Single(k => k.Key == "Cost*").Value;
						string purchaseCostWithoutUnits = Regex.Match(purchaseCostRawValue, @"\d+").Value;
						string purchaseCostUnits = (Regex.Matches(purchaseCostRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						long vehiclePurchaseCost = long.Parse(purchaseCostWithoutUnits);
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