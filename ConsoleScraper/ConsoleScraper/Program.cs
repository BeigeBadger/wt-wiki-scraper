﻿using System;
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
using System.Security.Cryptography;
using ConsoleScraper.Enums;

namespace ConsoleScraper
{
	/*
		TODO: More indepth errors list
		TODO: Name stuff better
		TODO: Think about vehicle scoping
		TODO: static vs const
		TODO: Make config parameters for constants eg path, whether to make local files etc
	*/

	class Program
	{
		public static string BaseWikiUrl = "http://wiki.warthunder.com/";
		public static string GroundForcesWikiUrl = $"{BaseWikiUrl}index.php?title=Category:Ground_vehicles";
		public static string LastModifiedSectionId = "footer-info-lastmod";

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

				VehicleCostUnitHelper vehicleCostUnitHelper = new VehicleCostUnitHelper();
				VehicleSpeedUnitHelper vehicleSpeedUnitHelper = new VehicleSpeedUnitHelper();
				VehicleWeightUnitHelper vehicleWeightUnitHelper = new VehicleWeightUnitHelper();
				VehicleCountryHelper vehicleCountryHelper = new VehicleCountryHelper();
				GroundVehicleTypeHelper vehicleTypeHelper = new GroundVehicleTypeHelper();
				VehicleEnginePowerUnitHelper vehicleEnginePowerUnitHelper = new VehicleEnginePowerUnitHelper();

				HtmlWeb webGet = new HtmlWeb();

				List<string> errorList = new List<string>();

				// Load Wiki Home page
				HtmlDocument groundForcesWikiHomePage = webGet.Load(GroundForcesWikiUrl);

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
					// Get "Pages in category "Ground vehicles"" section | <div id="mw-pages"> | document.getElementById('mw-pages')
					HtmlNode listContainerNode = groundForcesWikiHomePage.DocumentNode.Descendants().Single(d => d.Id == "mw-pages");
					// Get container that holds the table with the links | <div lang="en" dir="ltr" class="mw-content-ltr"> | document.getElementsByClassName('mw-content-ltr')[1]
					HtmlNode tableContainerNode = listContainerNode.Descendants("div").Single(d => d.Attributes["class"].Value.Contains("mw-content-ltr"));
					// Get Vehicle links | div > table > tbody > tr > td > ul > li > a | document.getElementsByClassName('mw-content-ltr')[1].getElementsByTagName('a')
					List<HtmlNode> vehicleWikiEntryLinks = tableContainerNode.Descendants("table").Single().Descendants("a").ToList();

					// Get totals for the number of links to expect, and the number found
					string totalEntriesTextBlock = listContainerNode.Descendants("p").Single().InnerText;
					int totalNumberOfLinksBasedOnPageText = int.Parse(Regex.Match(totalEntriesTextBlock, @"\d+").Value);
					int totalNumberOfLinksBasedOnDomTraversal = vehicleWikiEntryLinks.Count();

					// Setup thread-safe collections for processing
					ConcurrentDictionary<int, HtmlNode> linksToVehicleWikiPages = new ConcurrentDictionary<int, HtmlNode>();
					ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent = new ConcurrentDictionary<string, HtmlDocument>();
					ConcurrentDictionary<string, string> localFileChanges = new ConcurrentDictionary<string, string>();

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
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent, localFileChanges)),
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent, localFileChanges)),
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent, localFileChanges)),
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent, localFileChanges))
					};

					// Wait until we have crawled all of the pages
					Task.WaitAll(webCrawlerTasks);

					webCrawlerStopwatch.Stop();
					processingStopwatch.Start();

					int indexPosition = 1;
					Dictionary<string, GroundVehicle> vehicleDetails = new Dictionary<string, GroundVehicle>();

					ProcessWikiHtmlFiles(vehicleDetails, vehicleWikiPagesContent.Values, errorList, vehicleTypeHelper, vehicleWeightUnitHelper,
						vehicleSpeedUnitHelper, vehicleEnginePowerUnitHelper, vehicleCountryHelper, vehicleCostUnitHelper, indexPosition,
						totalNumberOfLinksBasedOnDomTraversal);

					processingStopwatch.Stop();

					Console.WriteLine("================================================================");

					// Write out errors
					if (errorList.Any())
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"The following error{(errorList.Count() > 1 ? "s were" : "was")} encountered");

						foreach (string error in errorList)
						{
							Console.WriteLine(error);
						}
					}

					Console.ResetColor();
					Console.WriteLine("================================================================");

					overallStopwatch.Stop();

					TimeSpan timeSpan = overallStopwatch.Elapsed;
					Console.WriteLine($"Completed in {timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}");
					Console.WriteLine($"Expected total: {totalNumberOfLinksBasedOnPageText}, Actual total: {totalNumberOfLinksBasedOnDomTraversal}");
					Console.WriteLine($"Vehicle objects created: {vehicleDetails.Count()} (Actual - Errors)");

					if(localFileChanges.Any())
					{
						Console.WriteLine();
						Console.WriteLine("The following changes were made to the local wiki files: ");

						foreach(string change in localFileChanges.Values)
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

		// TODO: Make parameters into true globals if possible
		/// <summary>
		/// This is called inside the worker tasks to crawl the pages asynchronously
		/// </summary>
		/// <param name="vehiclePageLinks">A dictionary that contains a indexer for the key, and a link to the wiki page for that vehicle as the value</param>
		/// <param name="vehicleWikiPageDocuments">An empty dictionary (used as a global) that will be populated with the vehicle name as the key and the HTML content of the page as the value</param>
		/// <param name="localFileChanges">An empty dictionary (used as a global) that is used to keep track of changes made to local files, vehicle name as the key, and the action performed as the value</param>
		public static void GetPageHtml(ConcurrentDictionary<int, HtmlNode> vehiclePageLinks, ConcurrentDictionary<string, HtmlDocument> vehicleWikiPageDocuments, ConcurrentDictionary<string, string> localFileChanges)
		{
			foreach (var vehiclePageLink in vehiclePageLinks)
			{
				// Remove the current node so that the other threads don't reprocess it
				HtmlNode tempNode;
				vehiclePageLinks.TryRemove(vehiclePageLink.Key, out tempNode);

				// Fetch page information
				HtmlNode linkNode = vehiclePageLink.Value;
				string wikiRelativeUrl = linkNode.Attributes.Single(l => l.Name == "href").Value;
				string vehicleWikiEntryFullUrl = $"{BaseWikiUrl}{wikiRelativeUrl}";
				string vehicleName = linkNode.InnerText;

				Console.WriteLine();
				Console.WriteLine($"Processing... {vehicleName}");
				Console.WriteLine($"Vehicle: {vehicleName}, Url: {vehicleWikiEntryFullUrl}");

				// Visit page and extract data
				HtmlWeb vehicleWebGet = new HtmlWeb();
				HtmlDocument vehicleWikiPage = vehicleWebGet.Load(vehicleWikiEntryFullUrl);

				// Add page to new dictionary used to extract further data
				vehicleWikiPageDocuments.TryAdd(vehicleName, vehicleWikiPage);

				Console.WriteLine(vehicleWikiPageDocuments.Count());

				// Update the local repo
				UpdateLocalStorageForOfflineUse(localFileChanges, vehicleWikiPage, vehicleName);
			}
		}

		/// <summary>
		/// Loops through all of the vehicle wiki links that have been provided, attempts to parse the parts that we are interested in -
		/// the vehicle details table, creates an object from that, then stores the data locally if a flag is set
		/// </summary>
		/// <param name="vehicleDetails">An empty dictionary (global) that is populated with the vehicle name and vehicle object</param>
		/// <param name="vehicleWikiPages">A dictionary that contains links to all of the wiki pages that need parsing</param>
		/// <param name="errorList">A list (global) that is used to hold any errors that occur</param>
		/// <param name="indexPosition">The current index we are up to processing - used for error messages</param>
		/// <param name="expectedNumberOfLinks">The expected number of links to process</param>
		private static void ProcessWikiHtmlFiles(Dictionary<string, GroundVehicle> vehicleDetails, ICollection<HtmlDocument> vehicleWikiPages, List<string> errorList, GroundVehicleTypeHelper vehicleTypeHelper, VehicleWeightUnitHelper vehicleWeightUnitHelper ,VehicleSpeedUnitHelper vehicleSpeedUnitHelper, VehicleEnginePowerUnitHelper vehicleEnginePowerUnitHelper, VehicleCountryHelper vehicleCountryHelper, VehicleCostUnitHelper vehicleCostUnitHelper, int indexPosition, int expectedNumberOfLinks)
		{
			foreach (HtmlDocument vehicleWikiPage in vehicleWikiPages)
			{
				// Get the header that holds the page title | document.getElementsByClassName('firstHeading')[0].firstChild.innerText
				HtmlNode pageTitle = vehicleWikiPage.DocumentNode.Descendants().Single(d => d.Id == "firstHeading").FirstChild;
				// Get the div that holds all of the content under the title section | document.getElementById('bodyContent')
				HtmlNode wikiBody = vehicleWikiPage.DocumentNode.Descendants().Single(d => d.Id == "bodyContent");
				// Get the div that holds the content on the RHS of the page where the information table is | document.getElementById('bodyContent').getElementsByClassName('right-area')
				HtmlNode rightHandContent = wikiBody.Descendants("div").Single(d => d.Attributes["class"] != null && d.Attributes["class"].Value.Contains("right-area"));
				// Get the able that holds all of the vehicle information | document.getElementsByClassName('flight-parameters')[0]
				HtmlNode infoBox = rightHandContent.Descendants("table").SingleOrDefault(d => d.Attributes["class"].Value.Contains("flight-parameters"));

				// Name
				string vehicleName = pageTitle.InnerText;

				if (infoBox == null)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Error processing item {indexPosition} of {expectedNumberOfLinks}");
					Console.ResetColor();

					errorList.Add($"No Information found for '{vehicleName}', proceeding to next vehicle");
					indexPosition++;
					continue;
				}
				else
				{
					Dictionary<string, string> vehicleAttributes = new Dictionary<string, string>();
					HtmlNodeCollection rows = infoBox.SelectNodes("tr");

					Console.WriteLine($"The following values were found for {vehicleName}");

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

					Console.ResetColor();

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
					HtmlNode lastModifiedSection = vehicleWikiPage.DocumentNode.Descendants().SingleOrDefault(x => x.Id == LastModifiedSectionId);
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
						SuperStructureArmourThickness = vehicleSuperstructureArmourThickness,
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

					string vehicleJson = Newtonsoft.Json.JsonConvert.SerializeObject(groundVehicle, Newtonsoft.Json.Formatting.Indented);

					// Add Json to local directory
					string fileName = vehicleName.Replace(' ', '_').Replace('/', '-');
					string folderPath = @"..\..\LocalWiki\JSON\";
					string filePath = $@"{folderPath}{fileName}.json";

					if (!File.Exists(filePath))
					{
						File.WriteAllText(filePath, vehicleJson);

						// Record addition of new item
						//localFileChanges.TryAdd(vehicleName, $"New vehicle '{fileName}' added to local wiki"); // Cannot currently happen due to scope of variable - needs refactor to occur when we create the HTML file
						Console.WriteLine($"New vehicle '{fileName}'  JSON added to local wiki");
					}
					else
					{
						string existingFileText = File.ReadAllText(filePath);

						string newLastModSection = groundVehicle.LastModified;
						GroundVehicle existingVehicle = Newtonsoft.Json.JsonConvert.DeserializeObject<GroundVehicle>(existingFileText);
						string oldLastModSection = existingVehicle?.LastModified;

						if (!AreLastModifiedTimesTheSame(oldLastModSection, newLastModSection))
						{
							File.WriteAllText(filePath, vehicleJson);

							// Record update of existing item
							//localFileChanges.TryAdd(vehicleName, $"Vehicle '{fileName}' updated in local wiki");
							Console.WriteLine($"Vehicle '{fileName}' updated in local wiki");
						}
					}

					//WikiEntry entry = new WikiEntry(vehicleName, vehicleWikiEntryFullUrl, VehicleTypeEnum.Ground, vehicleInfo);

					vehicleDetails.Add(vehicleName, groundVehicle);

					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"Processed item {indexPosition} of {expectedNumberOfLinks} successfully");
					Console.WriteLine();
					Console.ResetColor();
				}

				indexPosition++;
			}
		}

		// TODO: Make this handle creating local JSON files as well
		/// <summary>
		/// Adds/updates files in the LocalWiki folder
		/// </summary>
		/// <param name="localFileChanges">This is being changed to be a global so it will no longer need to be a parameter</param>
		/// <param name="vehicleWikiPage">The HTML content of the wiki page</param>
		/// <param name="vehicleName">The vehicle name of the current wiki page</param>
		private static void UpdateLocalStorageForOfflineUse(ConcurrentDictionary<string, string> localFileChanges, HtmlDocument vehicleWikiPage, string vehicleName)
		{
			// Make path to save the local copy of the wiki in so we can run it offline if needs be
			string fileName = vehicleName.Replace(' ', '_').Replace('/', '-');
			string folderPath = @"..\..\LocalWiki\HTML\";
			string filePath = $@"{folderPath}{fileName}.html";

			if (!File.Exists(filePath))
			{
				vehicleWikiPage.Save($"{filePath}", Encoding.UTF8);

				// Record addition of new item
				localFileChanges.TryAdd(vehicleName, $"New vehicle '{fileName}' added to local wiki");
				Console.WriteLine($"New vehicle '{fileName}' added to local wiki");
			}
			else
			{
				// TODO: Abstract this code block if possible
				string existingFileText = File.ReadAllText(filePath);

				//Create a fake document so we can use helper methods to traverse through the existing file as an HTML document
				HtmlDocument htmlDoc = new HtmlDocument();
				HtmlNode existingHtml = HtmlNode.CreateNode(existingFileText);
				htmlDoc.DocumentNode.AppendChild(existingHtml);

				var newLastModSection = vehicleWikiPage.DocumentNode.Descendants().SingleOrDefault(x => x.Id == LastModifiedSectionId);
				var oldLastModSection = existingHtml.OwnerDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == LastModifiedSectionId);

				if (newLastModSection != null && oldLastModSection != null)
				{
					if (!AreLastModifiedTimesTheSame(oldLastModSection.InnerHtml, newLastModSection.InnerHtml))
					{
						vehicleWikiPage.Save($"{filePath}", Encoding.UTF8);

						// Record update of existing item
						localFileChanges.TryAdd(vehicleName, $"Vehicle '{fileName}' updated in local wiki");
						Console.WriteLine($"Vehicle '{fileName}' updated in local wiki");
					}
				}
				else if (oldLastModSection == null)
				{
					// Add new item
					vehicleWikiPage.Save($"{filePath}", Encoding.UTF8);

					// Record addition of new item
					localFileChanges.TryAdd(vehicleName, $"New vehicle '{fileName}' added to local wiki");
					Console.WriteLine($"New vehicle '{fileName}' added to local wiki");
				}
				else
				{
					throw new InvalidOperationException($"Unable to find the '{LastModifiedSectionId}' section, information comparision failed. Most likely the ID of the last modified section has changed.");
				}
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
	}
}