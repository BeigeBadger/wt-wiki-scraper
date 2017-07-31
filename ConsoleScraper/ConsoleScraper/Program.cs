using ConsoleScraper.Logging;
using ConsoleScraper.Util;
using ConsoleScraper.Util.Crawlers;
using ConsoleScraper.Util.Processors;
using ConsoleScraper.Util.Scrapers;
using HtmlAgilityPack;
using System;
using System.Diagnostics;

namespace ConsoleScraper
{
	internal class Program
	{
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
				_dataProcessor.CrawlWikiSectionPagesForData(groundForcesWikiHomePage);

				OverallStopwatch.Stop();

				TimeSpan elapsedTime = OverallStopwatch.Elapsed;

				// TODO: Add console manage method for this
				_consoleManager.WriteTextLine($"Completed in {elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}");
				_consoleManager.WriteExitInstructions();
			}
			catch (Exception ex)
			{
				_consoleManager.WriteException($"The following exception was encounted: {ex.Message}\r\nException details: {ex.StackTrace}");
			}
		}
	}
}