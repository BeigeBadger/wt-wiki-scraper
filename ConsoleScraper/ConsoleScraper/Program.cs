using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using ConsoleScraper.Models;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace ConsoleScraper
{
	internal class Program
	{
		/** Thread-safe collections **/

		// Populated with the vehicle name as the key and the HTML content of the page as the value
		private static readonly ConcurrentDictionary<string, HtmlDocument> VehicleWikiPagesContent = new ConcurrentDictionary<string, HtmlDocument>();

		// Keeps track of changes made to local files, vehicle name as the key, and the action performed as the value
		private static readonly ConcurrentDictionary<string, string> LocalFileChanges = new ConcurrentDictionary<string, string>();

		// Populated with the vehicle name and vehicle objects
		private static readonly Dictionary<string, GroundVehicle> VehicleDetails = new Dictionary<string, GroundVehicle>();

		private static readonly List<string> ErrorsList = new List<string>();

		private static ConsoleManager _consoleManager;
		private static ExcelLogger _excelLogger;
		private static FilePerVehicleLogger _filePerVehicleLogger;
		private static HtmlLogger _htmlLogger;
		private static JsonLogger _jsonLogger;
		private static WebCrawler _webCrawler;
		private static StringHelper _stringHelper;
		private static Logger _logger;
		private static DataProcessor _dataProcessor;
		private static GroundForcesScraper _groundForcesScraper;

		private static bool _createJsonFiles = true;
		private static bool _createHtmlFiles = true;
		private static bool _createExcelFile = true;

		#region Debugging helpers

		private static readonly Stopwatch OverallStopwatch = new Stopwatch();

		#endregion Debugging helpers

		private static void Main()
		{
			_consoleManager = new ConsoleManager();
			_excelLogger = new ExcelLogger();
			_filePerVehicleLogger = new FilePerVehicleLogger(_consoleManager);
			_htmlLogger = new HtmlLogger(_filePerVehicleLogger, _consoleManager);
			_jsonLogger = new JsonLogger(_filePerVehicleLogger, _consoleManager);
			_webCrawler = new WebCrawler(_consoleManager);
			_stringHelper = new StringHelper();
			_logger = new Logger(_jsonLogger, _htmlLogger, _stringHelper, _consoleManager);
			_dataProcessor = new DataProcessor(_consoleManager, _stringHelper, _webCrawler, _excelLogger, _logger);
			_groundForcesScraper = new GroundForcesScraper(_webCrawler);

			try
			{
				OverallStopwatch.Start();

				_consoleManager.WriteProgramTitleVersionAndInitialBlurb();
				_consoleManager.WriteInputInstructionsAndAwaitUserInput(ConsoleColor.Yellow, ConsoleKey.Enter, "Press ENTER to begin.");

				// Load Wiki Home page
				HtmlDocument groundForcesWikiHomePage = _groundForcesScraper.GetGroundForcesWikiHomePage();

				// Crawl ground forces
				// TODO: Some of these parameters can be moved into DataProcessor as they aren't used again
				_dataProcessor.CrawlWikiSectionPagesForData(groundForcesWikiHomePage, VehicleWikiPagesContent, LocalFileChanges, VehicleDetails, ErrorsList, OverallStopwatch, _createJsonFiles, _createHtmlFiles, _createExcelFile);

				_consoleManager.WriteExitInstructions();
			}
			catch (Exception ex)
			{
				_consoleManager.WriteException($"The following exception was encounted: {ex.Message}\r\nException details: {ex.StackTrace}");
			}
		}
	}
}