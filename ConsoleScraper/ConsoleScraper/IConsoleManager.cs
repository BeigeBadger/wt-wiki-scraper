using System;

namespace ConsoleScraper
{
	public interface IConsoleManager
	{
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
	}

	public class ConsoleManager : IConsoleManager
	{
		public const string HorizontalSeparator = "================================================================";

		public ConsoleManager()
		{
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

	}
}