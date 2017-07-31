using ConsoleScraper.Util.Interfaces;
using HtmlAgilityPack;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace ConsoleScraper.Util
{
	public class ConsoleManager : IConsoleManager
	{
		private const string HorizontalSeparator = "================================================================";
		private readonly string _currentApplicationVersion = FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetEntryAssembly().Location).FileVersion;

		public ConsoleManager()
		{
		}

		public void HandleCreateFileTypePrompts(out bool createJsonFiles, out bool createHtmlFiles, out bool createExcelFile)
		{
			// TODO: Make more DRY
			WriteLineInColour(ConsoleColor.Yellow, "Would you like a JSON file to be created on your local machine for each vehicle that was found? Enter Y [default] or N.");
			createJsonFiles = IsPressedKeyExpectedKey(ConsoleKey.Y);
			string jsonPath = Path.GetFullPath(ConfigurationManager.AppSettings["LocalWikiJsonPath"]);
			WriteLineInColourFollowedByBlankLine(ConsoleColor.Green, $"JSON files will {(createJsonFiles ? "" : "not")} be created {(createJsonFiles ? $"in {jsonPath}" : "")}.");

			WriteLineInColour(ConsoleColor.Yellow, "Would you like an HTML file to be created on your local machine for each vehicle that was found? Enter Y [default] or N.");
			createHtmlFiles = IsPressedKeyExpectedKey(ConsoleKey.Y);
			string htmlPath = Path.GetFullPath(ConfigurationManager.AppSettings["LocalWikiHtmlPath"]);
			WriteLineInColourFollowedByBlankLine(ConsoleColor.Green, $"HTML files will {(createHtmlFiles ? "" : "not")} be created {(createHtmlFiles ? $"in {htmlPath}" : "")}.");

			WriteLineInColour(ConsoleColor.Yellow, "Would you like an Excel file to be created on your location machine with all of the vehicle data for the vehicles that were found? Enter Y [default] or N.");
			createExcelFile = IsPressedKeyExpectedKey(ConsoleKey.Y);
			string excelPath = Path.GetFullPath(ConfigurationManager.AppSettings["LocalWikiExcelPath"]);
			WriteLineInColourFollowedByBlankLine(ConsoleColor.Green, $"An Excel file will {(createExcelFile ? "" : "not")} be created {(createExcelFile ? $"in {excelPath}" : "")}.");
		}

		public void HandleHtmlParseErrors(HtmlDocument htmlDocument)
		{
			WriteLineInColourFollowedByBlankLine(ConsoleColor.Red, "The following errors were encountered:", false);

			foreach (HtmlParseError error in htmlDocument.ParseErrors)
			{
				WriteTextLine(error.Reason);
			}

			ResetConsoleTextColour();
		}

		public bool IsPressedKeyExpectedKey(ConsoleKey key)
		{
			return Console.ReadKey(true).Key == key;
		}

		public void ResetConsoleTextColour()
		{
			Console.ResetColor();
		}

		public void WaitUntilKeyIsPressed(ConsoleKey key)
		{
			while (true)
			{
				if (Console.ReadKey(true).Key == key)
				{
					WriteLineInColour(ConsoleColor.Green, "Correct key acknowledged, proceeding...");
					break;
				}
			}
		}

		public void WriteBlankLine()
		{
			Console.WriteLine();
		}

		public void WriteException(string exceptionMessage)
		{
			WritePaddedHorizontalSeparator();
			WriteLineInColour(ConsoleColor.Red, exceptionMessage);
			WritePaddedHorizontalSeparator();
			ResetConsoleTextColour();
		}

		public void WriteExitInstructions()
		{
			// Wait until the user hits 'Esc' to terminate the application
			WriteLineInColour(ConsoleColor.Yellow, "Press ESC to exit...");
			WaitUntilKeyIsPressed(ConsoleKey.Escape);
		}

		public void WriteHorizontalSeparator()
		{
			Console.WriteLine(HorizontalSeparator);
		}

		public void WritePaddedHorizontalSeparator()
		{
			WriteBlankLine();
			WriteHorizontalSeparator();
			WriteBlankLine();
		}

		public void WritePaddedText(string textToWrite)
		{
			WriteBlankLine();
			WriteTextLine(textToWrite);
			WriteBlankLine();
		}

		public void WriteProcessingSummary(int expectedLinksTotal, int foundLinksTotal, int vehicleObjectsCreated, int errorsEncountered)
		{
			WriteTextLine($"Expected total: {expectedLinksTotal}");
			WriteTextLine($"Actual total: {foundLinksTotal}");
			WriteTextLine($"Errors encountered: {errorsEncountered}");
			WriteTextLine($"Vehicle objects created: {vehicleObjectsCreated} (should be Actual - Errors)");
		}

		public void WriteTextLine(string textToWrite)
		{
			Console.WriteLine(textToWrite);
		}

		public void WriteLineInColour(ConsoleColor colour, string textToWrite, bool resetColour = true)
		{
			Console.ForegroundColor = colour;
			Console.WriteLine(textToWrite);

			if (resetColour)
				Console.ResetColor();
		}

		public void WriteLineInColourFollowedByBlankLine(ConsoleColor colour, string textToWrite, bool resetColour = true)
		{
			WriteLineInColour(colour, textToWrite, resetColour);
			WriteBlankLine();
		}

		public void WriteLineInColourPreceededByBlankLine(ConsoleColor colour, string textToWrite, bool resetColour = true)
		{
			WriteBlankLine();
			WriteLineInColour(colour, textToWrite, resetColour);
		}

		public void WriteProgramTitleVersionAndInitialBlurb()
		{
			WriteLineInColour(ConsoleColor.Green, $"War Thunder Wiki Scraper v{_currentApplicationVersion}");
			WriteHorizontalSeparator();
			WritePaddedText("Blurb goes here...");
		}

		public void WriteInputInstructionsAndAwaitUserInput(ConsoleColor textColour, ConsoleKey expectedKey, string inputInstructions)
		{
			WriteLineInColour(textColour, inputInstructions);
			WaitUntilKeyIsPressed(expectedKey);
			WriteBlankLine();
		}
	}
}