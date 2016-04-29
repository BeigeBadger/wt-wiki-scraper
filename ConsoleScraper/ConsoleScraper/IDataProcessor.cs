using ConsoleScraper.Enums;
using ConsoleScraper.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleScraper
{
	public interface IDataProcessor
	{
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
		void ProcessWikiHtmlFiles(ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent, ConcurrentDictionary<string, string> localFileChanges, Dictionary<string, GroundVehicle> vehicleDetails, List<HtmlNode> vehicleWikiEntryLinks, List<string> errorsList, int indexPosition, int expectedNumberOfLinks, bool createJsonFiles, bool createHtmlFiles, bool createExcelFile);
	}

	public class DataProcessor : IDataProcessor
	{
		IConsoleManager _consoleManager;
		IStringHelper _stringHelper;
		IWebCrawler _webCrawler;
		IExcelLogger _excelLogger;
		ILogger _logger;

		// Helper objects
		VehicleCostUnitHelper _vehicleCostUnitHelper = new VehicleCostUnitHelper();
		VehicleSpeedUnitHelper _vehicleSpeedUnitHelper = new VehicleSpeedUnitHelper();
		VehicleWeightUnitHelper _vehicleWeightUnitHelper = new VehicleWeightUnitHelper();
		VehicleCountryHelper _vehicleCountryHelper = new VehicleCountryHelper();
		GroundVehicleTypeHelper _vehicleTypeHelper = new GroundVehicleTypeHelper();
		VehicleEnginePowerUnitHelper _vehicleEnginePowerUnitHelper = new VehicleEnginePowerUnitHelper();

		public DataProcessor(IConsoleManager consoleManager, IStringHelper stringHelper, IWebCrawler webCrawler, IExcelLogger excelLogger, ILogger logger)
		{
			_consoleManager = consoleManager;
			_stringHelper = stringHelper;
			_webCrawler = webCrawler;
			_excelLogger = excelLogger;
			_logger = logger;
		}

		public void ProcessWikiHtmlFiles(ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent, ConcurrentDictionary<string, string> localFileChanges, Dictionary<string, GroundVehicle> vehicleDetails, List<HtmlNode> vehicleWikiEntryLinks, List<string> errorsList, int indexPosition, int expectedNumberOfLinks, bool createJsonFiles, bool createHtmlFiles, bool createExcelFile)
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
					HtmlNode infoBox = rightHandContent != null
						? rightHandContent.Descendants("table").SingleOrDefault(d => d.Attributes["class"].Value.Contains("flight-parameters"))
						: null;

					// Name
					string vehicleName = _stringHelper.RemoveInvalidCharacters(System.Net.WebUtility.HtmlDecode(vehicleWikiPageLinkTitle));

					// Link
					HtmlNode urlNode = vehicleWikiEntryLinks.SingleOrDefault(v => v.InnerText.Equals(vehicleName));
					string relativeUrl = urlNode != null
						? urlNode.Attributes["href"].Value.ToString()
						: "";
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
						string countryRawValue = vehicleAttributes.Single(k => k.Key == "Country").Value.ToString();
						CountryEnum vehicleCountry = _vehicleCountryHelper.GetVehicleCountryFromName(countryRawValue).CountryEnum;

						// Weight
						string weightRawValue = vehicleAttributes.Single(k => k.Key == "Weight").Value.ToString();
						int weightWithoutUnits = int.Parse(Regex.Match(weightRawValue, @"\d+").Value);
						string weightUnitsAbbreviation = (Regex.Matches(weightRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleWeightUnitHelper vehicleWeightUnit = _vehicleWeightUnitHelper.GetWeightUnitFromAbbreviation(weightUnitsAbbreviation);

						// Vehicle class
						string typeRawValue = vehicleAttributes.Single(k => k.Key == "Type").Value.ToString();
						GroundVehicleTypeHelper vehicleType = _vehicleTypeHelper.GetGroundVehicleTypeFromName(typeRawValue);

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
						VehicleEnginePowerUnitHelper vehicleEngineUnit = _vehicleEnginePowerUnitHelper.GetEngineUnitFromAbbreviation(enginePowerUnitsAbbreviation);

						// Max speed
						string maxSpeedRawValue = vehicleAttributes.Single(k => k.Key == "Max speed").Value.ToString();
						double maxSpeedWithoutUnits = double.Parse(Regex.Match(maxSpeedRawValue, @"\d+\.*\d*").Value);
						string maxSpeedUnits = (Regex.Matches(maxSpeedRawValue, @"\D+").Cast<Match>()).Last().Value.Trim();
						VehicleSpeedUnitHelper vehicleSpeedUnit = _vehicleSpeedUnitHelper.GetSpeedUnitFromAbbreviation(maxSpeedUnits);

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
						VehicleCostUnitHelper vehicleRepairCostUnit = _vehicleCostUnitHelper.GetCostUnitFromAbbreviation(maxRepairCostUnits);

						// Purchase cost
						string purchaseCostRawValue = vehicleAttributes.Single(k => k.Key == "Cost*").Value.ToString();
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
						if (createJsonFiles)
						{
							_logger.UpdateLocalStorageForOfflineUse(localFileChanges, vehicleWikiPage, vehicleName, LocalWikiFileTypeEnum.JSON, groundVehicle);
						}

						if (createHtmlFiles)
						{
							_logger.UpdateLocalStorageForOfflineUse(localFileChanges, vehicleWikiPage, vehicleName, LocalWikiFileTypeEnum.HTML, null);
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