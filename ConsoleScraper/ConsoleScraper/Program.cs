using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using ConsoleScraper.Models;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ConsoleScraper
{
	/*
		TODO: More indepth errors list
		TODO: Name stuff better
		TODO: Think about vehicle scoping
		TODO: static vs const
	*/

	class Program
	{
		public static string BaseWikiUrl = "http://wiki.warthunder.com/";
		public static string GroundForcesWikiUrl = $"{BaseWikiUrl}index.php?title=Category:Ground_vehicles";

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

				VehicleCostUnit vehicleCostUnit = new VehicleCostUnit();
				VehicleSpeedUnit vehicleSpeedUnit = new VehicleSpeedUnit();
				VehicleWeightUnit vehicleWeightUnit = new VehicleWeightUnit();
				VehicleEnginePowerUnit vehicleEnginePowerUnit = new VehicleEnginePowerUnit();

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
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent)),
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent)),
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent)),
						Task.Factory.StartNew(() => GetPageHtml(linksToVehicleWikiPages, vehicleWikiPagesContent))
					};

					// Wait until we have crawled all of the pages
					Task.WaitAll(webCrawlerTasks);

					webCrawlerStopwatch.Stop();
					processingStopwatch.Start();


					int indexPosition = 1;
					Dictionary<string, GroundVehicle> vehicleDetails = new Dictionary<string, GroundVehicle>();

					foreach (HtmlDocument vehicleWikiPage in vehicleWikiPagesContent.Values)
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
							Console.WriteLine($"Error processing item {indexPosition} of {totalNumberOfLinksBasedOnDomTraversal}");
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

							// Weight
							string weightRawValue = vehicleAttributes.Single(k => k.Key == "Weight").Value.ToString();
							int weightWithoutUnits = int.Parse(Regex.Match(weightRawValue, @"\d+").Value);
							string weightUnitsAbbreviation = (Regex.Matches(weightRawValue, @"\D+").Cast<Match>()).Last().Value;
							vehicleWeightUnit.GetWeightUnitFromAbbreviation(weightUnitsAbbreviation);

							// Vehicle class
							string typeRawValue = vehicleAttributes.Single(k => k.Key == "Type").Value.ToString();

							// Rank
							int rankRawValue = int.Parse(vehicleAttributes.Single(k => k.Key == "Rank").Value.ToString());

							// Battle rating
							double ratingRawValue = double.Parse(vehicleAttributes.Single(k => k.Key == "Rating").Value.ToString());

							// Engine power
							string enginePowerRawValue = vehicleAttributes.Single(k => k.Key == "Engine power").Value.ToString();
							int enginePowerWithoutUnits = int.Parse(Regex.Match(enginePowerRawValue, @"\d+").Value);
							string enginePowerUnitsAbbreviation = (Regex.Matches(enginePowerRawValue, @"\D+").Cast<Match>()).Last().Value;
							vehicleEnginePowerUnit.GetEngineUnitFromAbbreviation(enginePowerUnitsAbbreviation);

							// Max speed
							string maxSpeedRawValue = vehicleAttributes.Single(k => k.Key == "Max speed").Value.ToString();
							double maxSpeedWithoutUnits = double.Parse(Regex.Match(maxSpeedRawValue, @"\d+\.*\d*").Value);
							string maxSpeedUnits = (Regex.Matches(maxSpeedRawValue, @"\D+").Cast<Match>()).Last().Value;
							vehicleSpeedUnit.GetSpeedUnitFromAbbreviation(maxSpeedUnits);

							// Hull armour
							string hullArmourRawValue = vehicleAttributes.Single(k => k.Key == "Hull armour thickness").Value.ToString();

							// Superstructure armour
							string superstructureArmourRawValue = vehicleAttributes.Single(k => k.Key == "Superstructure armour thickness").Value.ToString();

							// Repair time
							string freeRepairTimeRawValue = vehicleAttributes.Single(k => k.Key == "Time for free repair").Value.ToString();
							List<Match> freeRepairTimeList = (Regex.Matches(freeRepairTimeRawValue, @"\d+").Cast<Match>()).ToList();
							int freeRepairTimeHours = int.Parse(freeRepairTimeList.First().Value);
							int freeRepairTimeMinutes = int.Parse(freeRepairTimeList.Last().Value);
							TimeSpan freeRepairTime = new TimeSpan(freeRepairTimeHours, freeRepairTimeMinutes, 0);

							// Timespan.parse

							// Max repair cost
							string maxRepairCostRawValue = vehicleAttributes.Single(k => k.Key == "Max repair cost*").Value.ToString();
							string maxRepairCostWithoutUnits = Regex.Match(maxRepairCostRawValue, @"\d+").Value;
							string maxRepairCostUnits = (Regex.Matches(maxRepairCostRawValue, @"\D+").Cast<Match>()).Last().Value;
							vehicleCostUnit.GetCostUnitFromAbbreviation(maxRepairCostUnits);

							// Purchase cost
							string purchaseCostRawValue = vehicleAttributes.Single(k => k.Key == "Cost*").Value.ToString();
							string purchaseCostWithoutUnits = Regex.Match(purchaseCostRawValue, @"\d+").Value;
							string purchaseCostUnits = (Regex.Matches(purchaseCostRawValue, @"\D+").Cast<Match>()).Last().Value;
							vehicleCostUnit.GetCostUnitFromAbbreviation(purchaseCostUnits);

							// Populate objects
							GroundVehicle groundVehicle = new GroundVehicle
							{
								Title = vehicleName,
								//Country = ,
								//Weight = weightWithoutUnits

							};

							//WikiEntry entry = new WikiEntry(vehicleName, vehicleWikiEntryFullUrl, VehicleTypeEnum.Ground, vehicleInfo);

							vehicleDetails.Add(vehicleName, groundVehicle);

							Console.ForegroundColor = ConsoleColor.Green;
							Console.WriteLine($"Processed item {indexPosition} of {totalNumberOfLinksBasedOnDomTraversal} successfully");
							Console.WriteLine();
							Console.ResetColor();
						}

						indexPosition++;
					}

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

		public static void GetPageHtml(ConcurrentDictionary<int, HtmlNode> vehiclePageLinks, ConcurrentDictionary<string, HtmlDocument> vehicleWikiPageDocuments)
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
			}
		}
	}
}