using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ConsoleScraper.Models;
using System.Diagnostics;
using System.Collections.Concurrent;

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
		public static GroundForcesScraper GroundForcesScraper;

		public static bool CreateJsonFiles = true;
		public static bool CreateHtmlFiles = true;
		public static bool CreateExcelFile = true;

		#region Debugging helpers

		public static Stopwatch overallStopwatch = new Stopwatch();

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
			GroundForcesScraper = new GroundForcesScraper(WebCrawler, ConsoleManager);

			try
			{
				overallStopwatch.Start();

				ConsoleManager.WriteProgramTitleVersionAndInitialBlurb();
				ConsoleManager.WriteInputInstructionsAndAwaitUserInput(ConsoleColor.Yellow, ConsoleKey.Enter, "Press ENTER to begin.");

				// Load Wiki Home page
				HtmlDocument groundForcesWikiHomePage = GroundForcesScraper.GetGroundForcesWikiHomePage();

				// Crawl ground forces
				// TODO: Move of these parameters can be moved into DataProcessor as they aren't used againw
				DataProcessor.CrawlWikiSectionPagesForData(groundForcesWikiHomePage, vehicleWikiPagesContent, localFileChanges, vehicleDetails, errorsList,
					overallStopwatch, CreateJsonFiles, CreateHtmlFiles, CreateExcelFile);

				ConsoleManager.WriteExitInstructions();
			}
			catch (Exception ex)
			{
				ConsoleManager.WriteException($"The following exception was encounted: {ex.Message}\r\nException details: {ex.StackTrace}");
			}
		}
	}
}