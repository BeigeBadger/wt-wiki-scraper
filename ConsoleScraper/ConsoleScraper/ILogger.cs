using ConsoleScraper.Enums;
using ConsoleScraper.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;

namespace ConsoleScraper
{
	public interface ILogger
	{
		/// <summary>
		/// Adds/updates files in the LocalWiki folder
		/// </summary>
		/// <param name="localFileChanges">Keeps track of changes made to local files, vehicle name as the key, and the action performed as the value</param>
		/// <param name="vehicleWikiPage">The HTML content of the wiki page</param>
		/// <param name="vehicleName">The vehicle name of the current wiki page</param>
		void UpdateLocalStorageForOfflineUse(ConcurrentDictionary<string, string> localFileChanges, HtmlDocument vehicleWikiPage, string vehicleName, LocalWikiFileTypeEnum fileType, IVehicle vehicle = null);
	}

	public class Logger : ILogger
	{
		IJsonLogger _jsonLogger;
		IHtmlLogger _htmlLogger;
		IStringHelper _stringHelper;
		IConsoleManager _consoleManager;

		public Logger(IJsonLogger jsonLogger, IHtmlLogger htmlLogger, IStringHelper stringHelper, IConsoleManager consoleManager)
		{
			_jsonLogger = jsonLogger;
			_htmlLogger = htmlLogger;
			_stringHelper = stringHelper;
			_consoleManager = consoleManager;
		}

		public void UpdateLocalStorageForOfflineUse(ConcurrentDictionary<string, string> localFileChanges, HtmlDocument vehicleWikiPage, string vehicleName, LocalWikiFileTypeEnum fileType, IVehicle vehicle = null)
		{
			try
			{
				if (fileType == LocalWikiFileTypeEnum.Undefined)
					throw new ArgumentException("The 'fileType' parameter for the 'UpdateLocalStorageForOfflineUse' is required but was not provided.");

				// Build vars that will be used for the local file
				string fileName = _stringHelper.RemoveInvalidCharacters(vehicleName.Replace(' ', '_').Replace('/', '-'));
				string folderPath = fileType == LocalWikiFileTypeEnum.HTML ? ConfigurationManager.AppSettings["LocalWikiHtmlPath"] : ConfigurationManager.AppSettings["LocalWikiJsonPath"];
				string filePath = $@"{folderPath}{fileName}.{fileType.ToString().ToLower()}";

				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				// Handle HTML files
				if (fileType == LocalWikiFileTypeEnum.HTML)
				{
					_htmlLogger.CreateHtmlFile(localFileChanges, vehicleWikiPage, vehicleName, fileName, filePath);
				}
				// Handle JSON files
				else if (fileType == LocalWikiFileTypeEnum.JSON)
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