using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using ConsoleScraper.Models;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace ConsoleScraper
{
	class Program
	{
		/** Thread-safe collections **/
		// Populated with the vehicle name as the key and the HTML content of the page as the value
		public static ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent = new ConcurrentDictionary<string, HtmlDocument>();
		// Keeps track of changes made to local files, vehicle name as the key, and the action performed as the value
		public static ConcurrentDictionary<string, string> localFileChanges = new ConcurrentDictionary<string, string>();
		// Populated with the vehicle name and vehicle objects
		public static Dictionary<string, GroundVehicle> vehicleDetails = new Dictionary<string, GroundVehicle>();

		public static List<string> errorsList = new List<string>();

		public static ConsoleManager ConsoleManager;
		public static ExcelLogger ExcelLogger;
		public static FilePerVehicleLogger FilePerVehicleLogger;
		public static HtmlLogger HtmlLogger;
		public static JsonLogger JsonLogger;
		public static WebCrawler WebCrawler;
		public static StringHelper StringHelper;
		public static Logger Logger;
		public static DataProcessor DataProcessor;


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
			HtmlLogger = new HtmlLogger(FilePerVehicleLogger, ConsoleManager);
			JsonLogger = new JsonLogger(FilePerVehicleLogger, ConsoleManager);
			WebCrawler = new WebCrawler(ConsoleManager);
			StringHelper = new StringHelper();
			Logger = new Logger(JsonLogger, HtmlLogger, StringHelper, ConsoleManager);
			DataProcessor = new DataProcessor(ConsoleManager, StringHelper, WebCrawler, ExcelLogger, Logger);

			try
			{
				overallStopwatch.Start();

				ConsoleManager.WriteProgramTitleVersionAndInitialBlurb();
				ConsoleManager.WriteInputInstructionsAndAwaitUserInput(ConsoleColor.Yellow, ConsoleKey.Enter, "Press ENTER to begin.");

				HtmlWeb webGet = new HtmlWeb();

				// Load Wiki Home page
				HtmlDocument groundForcesWikiHomePage = webGet.Load(ConfigurationManager.AppSettings["GroundForcesWikiUrl"]);

				// Fail fast if there are errors
				if (groundForcesWikiHomePage.ParseErrors != null && groundForcesWikiHomePage.ParseErrors.Any())
				{
					ConsoleManager.WriteLineInColourFollowedByBlankLine(ConsoleColor.Red, "The following errors were encountered:", false);

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
					ConsoleManager.WriteInputInstructionsAndAwaitUserInput(ConsoleColor.Yellow, ConsoleKey.Enter, "Press ENTER to begin searching for links to vehicle pages.");

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
					ConsoleManager.WriteLineInColourFollowedByBlankLine(ConsoleColor.Green, $"Will{(CreateJsonFiles ? " " : " not ")}create JSON files.");

					ConsoleManager.WriteLineInColour(ConsoleColor.Yellow, "Would you like to create HTML files for each vehicle locally? Enter Y [default] or N.");
					CreateHtmlFiles = ConsoleManager.IsPressedKeyExpectedKey(ConsoleKey.Y);
					ConsoleManager.WriteLineInColourFollowedByBlankLine(ConsoleColor.Green, $"Will{(CreateHtmlFiles ? " " : " not ")}create HTML files.");

					ConsoleManager.WriteLineInColour(ConsoleColor.Yellow, "Would you like to create an Excel file with all of the vehicle data? Enter Y [default] or N.");
					CreateExcelFile = ConsoleManager.IsPressedKeyExpectedKey(ConsoleKey.Y);
					ConsoleManager.WriteLineInColourFollowedByBlankLine(ConsoleColor.Green, $"Will{(CreateExcelFile ? " " : " not ")}create Excel file.");

					int indexPosition = 1;

					processingStopwatch.Start();

					// Extract information from the pages we've traversed
					DataProcessor.ProcessWikiHtmlFiles(vehicleWikiPagesContent, localFileChanges, vehicleDetails, vehicleWikiEntryLinks, errorsList, indexPosition, totalNumberOfLinksBasedOnPageText, CreateJsonFiles, CreateHtmlFiles, CreateExcelFile);

					processingStopwatch.Stop();

					ConsoleManager.WriteLineInColourPreceededByBlankLine(ConsoleColor.Green, $"Finished processing html files for vehicle data{(CreateExcelFile || CreateHtmlFiles || CreateJsonFiles ? " and writing local changes." : ".")}");

					// Write out local file changes
					if (localFileChanges.Any())
					{
						string localChangesFilePath = $"{ConfigurationManager.AppSettings["LocalWikiRootPath"].ToString()}Changes.txt";
						Dictionary<string, string> orderedLocalFileChanges = localFileChanges.OrderBy(x => x.Key).ToDictionary(d => d.Key, d => d.Value);

						ConsoleManager.WritePaddedText("The following changes were made to the local wiki files: ");

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
					if (errorsList.Any())
					{
						string errorFilePath = $"{ConfigurationManager.AppSettings["LocalWikiRootPath"].ToString()}Errors.txt";

						ConsoleManager.WriteLineInColour(ConsoleColor.Red, $"The following error{(errorsList.Count() > 1 ? "s were" : "was")} encountered:", false);

						using (StreamWriter streamWriter = File.CreateText(errorFilePath))
						{
							foreach (string error in errorsList)
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
				ConsoleManager.WriteLineInColour(ConsoleColor.Yellow, "Press ESC to exit...");
				ConsoleManager.WaitUntilKeyIsPressed(ConsoleKey.Escape);
			}
			catch (Exception ex)
			{
				ConsoleManager.WriteException($"The following exception was encounted: {ex.Message}\r\nException details: {ex.StackTrace}");
			}
		}
	}
}