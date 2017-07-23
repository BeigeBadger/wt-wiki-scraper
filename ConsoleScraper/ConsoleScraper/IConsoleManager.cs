using HtmlAgilityPack;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace ConsoleScraper
{
	public interface IConsoleManager
	{
		/// <summary>
		/// Will ask the user a series of questions about whether they want to create
		/// certain file types after processing the wiki data
		/// </summary>
		/// <param name="createJsonFiles">Initial value of whether we want to create JSON files</param>
		/// <param name="createHtmlFiles">Initial value of whether we want to create HTML files</param>
		/// <param name="createExcelFiles">Initial value of whether we want to create EXCEL files</param>
		void HandleCreateFileTypePrompts(out bool createJsonFiles, out bool createHtmlFiles, out bool createExcelFiles);

		/// <summary>
		/// Writes out any parse errors to the console
		/// </summary>
		/// <param name="htmlDocument">The document to handle the parse errors for</param>
		void HandleHtmlParseErrors(HtmlDocument htmlDocument);

		/// <summary>
		/// Checks the pressed to key to see if it was the expected one
		/// </summary>
		/// <param name="key">Key that is expected</param>
		/// <returns>Whether or not the user input matches the expected key</returns>
		bool IsPressedKeyExpectedKey(ConsoleKey key);

		/// <summary>
		/// Calls the inbuilt Console.ResetColor method to set the
		/// colour of the console text back to the default value
		/// </summary>
		void ResetConsoleTextColour();

		/// <summary>
		/// Blocks thread until the specified key is pressed
		/// </summary>
		/// <param name="key">Key press to wait for</param>
		void WaitUntilKeyIsPressed(ConsoleKey key);

		/// <summary>
		/// Calls the inbuilt Console.WriteLine method with
		/// no arguments, will just write a line with nothing on it
		/// </summary>
		void WriteBlankLine();

		/// <summary>
		/// Writes out the exception message in red text
		/// </summary>
		/// <param name="exceptionMessage">The exception message to write</param>
		void WriteException(string exceptionMessage);

		/// <summary>
		/// Writes instructions on how to terminate the application
		/// </summary>
		void WriteExitInstructions();

		/// <summary>
		/// Adds a horizontal bar to split up text sections in the console
		/// eg. =========================================================
		/// works like the HTML HR tag
		/// </summary>
		void WriteHorizontalSeparator();

		/// <summary>
		/// Works the same as AddHorizontalSeparator except there is a
		/// blank line on either side of the separator to give it more
		/// emphasis
		/// </summary>
		void WritePaddedHorizontalSeparator();

		/// <summary>
		/// Writes the specified text with a blank line either side
		/// using the inbuilt Console.WriteLine method
		/// </summary>
		/// <param name="textToWrite">The text to write</param>
		void WritePaddedText(string textToWrite);

		/// <summary>
		/// Writes out how long the program took to execute and other statistics
		/// </summary>
		/// <param name="runTime">How long it took to run</param>
		/// <param name="expectedLinksTotal">The number of links we expected to find (taken from page text)</param>
		/// <param name="foundLinksTotal">The number of links we actually found (via scraping)</param>
		/// <param name="vehicleObjectsCreated">How many vehicle objects were created</param>
		/// <param name="errorsEncountered">How many errors were encountered</param>
		void WriteProcessingSummary(TimeSpan runTime, int expectedLinksTotal, int foundLinksTotal, int vehicleObjectsCreated, int errorsEncountered);

		/// <summary>
		/// Will write the specified text to the console using the
		/// inbuilt Console.WriteLine method
		/// </summary>
		/// <param name="textToWrite">The text to write</param>
		void WriteTextLine(string textToWrite);

		/// <summary>
		/// Writes the specified text in the specified colour, with option to reset the text colour after
		/// </summary>
		/// <param name="colour">Colour to write the text in</param>
		/// <param name="textToWrite">The text to write</param>
		/// <param name="resetColour">Whether or not the reset the colour after writing</param>
		void WriteLineInColour(ConsoleColor colour, string textToWrite, bool resetColour = true);

		/// <summary>
		/// Writes the specified text in the specified colour, followed by a blank line,
		/// with option to reset the text colour after
		/// </summary>
		/// <param name="colour">Colour to write the text in</param>
		/// <param name="textToWrite">The text to write</param>
		/// <param name="resetColour">Whether or not the reset the colour after writing</param>
		void WriteLineInColourFollowedByBlankLine(ConsoleColor colour, string textToWrite, bool resetColour = true);

		/// <summary>
		/// Writes the specified text in the specified colour, preceeded by a blank line,
		/// with option to reset the text colour after
		/// </summary>
		/// <param name="colour">Colour to write the text in</param>
		/// <param name="textToWrite">The text to write</param>
		/// <param name="resetColour">Whether or not the reset the colour after writing</param>
		void WriteLineInColourPreceededByBlankLine(ConsoleColor colour, string textToWrite, bool resetColour = true);

		/// <summary>
		/// Writes out the application name, version and the initial description of the program
		/// </summary>
		void WriteProgramTitleVersionAndInitialBlurb();

		/// <summary>
		/// Writes out text in the specified colour, then awaits user input
		/// </summary>
		/// <param name="textColour">Colour to write the text in</param>
		/// <param name="expectedKey">The key we are expecting the user to press</param>
		/// <param name="inputInstructions">Text to write out</param>
		/// <returns>Whether or not the user input matches the expected key</returns>
		void WriteInputInstructionsAndAwaitUserInput(ConsoleColor textColour, ConsoleKey expectedKey, string inputInstructions);
	}

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

		public void WriteProcessingSummary(TimeSpan runTime, int expectedLinksTotal, int foundLinksTotal, int vehicleObjectsCreated, int errorsEncountered)
		{
			WriteTextLine($"Completed in {runTime.Hours:00}:{runTime.Minutes:00}:{runTime.Seconds:00}");
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