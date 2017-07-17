using ConsoleScraper.Enums;
using ConsoleScraper.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace ConsoleScraper
{
	public interface ILogger
	{
		/// <summary>
		/// Writes out any changes that were made to local files to the console and file
		/// </summary>
		/// <param name="localFileChanges">The list with the local file changes in it</param>
		void HandleLocalFileChanges(ConcurrentDictionary<string, string> localFileChanges);

		/// <summary>
		/// Writes out any errors that were encountered while trying to process data
		/// </summary>
		/// <param name="errorsList">The list of errors to write to console and file</param>
		void HandleProcessingErrors(List<string> errorsList);

		/// <summary>
		/// Adds/updates files in the LocalWiki folder
		/// </summary>
		/// <param name="localFileChanges">Keeps track of changes made to local files, vehicle name as the key, and the action performed as the value</param>
		/// <param name="vehicleWikiPage">The HTML content of the wiki page</param>
		/// <param name="vehicleName">The vehicle name of the current wiki page</param>
		/// <param name="fileType">The file type to update</param>
		/// <param name="vehicle">The vehicle details will be updated for</param>
		void UpdateLocalStorageForOfflineUse(ConcurrentDictionary<string, string> localFileChanges, HtmlDocument vehicleWikiPage, string vehicleName, LocalWikiFileTypeEnum fileType, IVehicle vehicle = null);
	}

	public class Logger : ILogger
	{
		private readonly IJsonLogger _jsonLogger;
		private readonly IHtmlLogger _htmlLogger;
		private readonly IStringHelper _stringHelper;
		private readonly IConsoleManager _consoleManager;

		public Logger(IJsonLogger jsonLogger, IHtmlLogger htmlLogger, IStringHelper stringHelper, IConsoleManager consoleManager)
		{
			_jsonLogger = jsonLogger;
			_htmlLogger = htmlLogger;
			_stringHelper = stringHelper;
			_consoleManager = consoleManager;
		}

		public void HandleLocalFileChanges(ConcurrentDictionary<string, string> localFileChanges)
		{
			string localChangesFilePath = $"{ConfigurationManager.AppSettings["LocalWikiRootPath"]}Changes.txt";
			Dictionary<string, string> orderedLocalFileChanges = localFileChanges.OrderBy(x => x.Key).ToDictionary(d => d.Key, d => d.Value);

			_consoleManager.WritePaddedText("The following changes were made to the local wiki files: ");

			using (StreamWriter streamWriter = File.CreateText(localChangesFilePath))
			{
				foreach (string change in orderedLocalFileChanges.Values)
				{
					_consoleManager.WriteTextLine(change);
					streamWriter.WriteLine(change);
				}
			}
		}

		public void HandleProcessingErrors(List<string> errorsList)
		{
			string errorFilePath = $"{ConfigurationManager.AppSettings["LocalWikiRootPath"]}Errors.txt";

			_consoleManager.WriteLineInColour(ConsoleColor.Red, $"The following error{(errorsList.Count > 1 ? "s were" : "was")} encountered:", false);

			using (StreamWriter streamWriter = File.CreateText(errorFilePath))
			{
				foreach (string error in errorsList)
				{
					_consoleManager.WriteTextLine(error);
					streamWriter.WriteLine(error);
				}
			}

			_consoleManager.ResetConsoleTextColour();
		}

		public void UpdateLocalStorageForOfflineUse(ConcurrentDictionary<string, string> localFileChanges, HtmlDocument vehicleWikiPage, string vehicleName, LocalWikiFileTypeEnum fileType, IVehicle vehicle = null)
		{
			try
			{
				if (fileType == LocalWikiFileTypeEnum.Undefined)
					throw new ArgumentException("The 'fileType' parameter for the 'UpdateLocalStorageForOfflineUse' is required but was not provided.");

				// Build vars that will be used for the local file
				string fileName = _stringHelper.RemoveInvalidCharacters(vehicleName.Replace(' ', '_').Replace('/', '-'));
				string folderPath = fileType == LocalWikiFileTypeEnum.Html ? ConfigurationManager.AppSettings["LocalWikiHtmlPath"] : ConfigurationManager.AppSettings["LocalWikiJsonPath"];
				string filePath = $@"{folderPath}{fileName}.{fileType.ToString().ToLower()}";

				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				// Handle HTML files
				if (fileType == LocalWikiFileTypeEnum.Html)
				{
					_htmlLogger.CreateHtmlFile(localFileChanges, vehicleWikiPage, vehicleName, fileName, filePath);
				}
				// Handle JSON files
				else if (fileType == LocalWikiFileTypeEnum.Json)
				{
					_jsonLogger.CreateJsonFile(localFileChanges, vehicleName, vehicle, fileName, filePath);
				}
			}
			catch (Exception ex)
			{
				_consoleManager.WriteException(ex.Message);
			}
		}
	}
}