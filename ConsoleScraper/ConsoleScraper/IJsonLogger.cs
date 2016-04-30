using ConsoleScraper.Enums;
using ConsoleScraper.Models;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;

namespace ConsoleScraper
{
	public interface IJsonLogger
	{
		/// <summary>
		/// Creates a JSON file in the local wiki using info
		/// scraped from the wiki entry
		/// </summary>
		/// <param name="localFileChanges">The dictionary that holds all of the local changes</param>
		/// <param name="vehicleName">Name of the vehicle the page is for</param>
		/// <param name="fileName">What the file should be called</param>
		/// <param name="filePath">Where the file should be stored</param>
		void CreateJsonFile(ConcurrentDictionary<string, string> localFileChanges, string vehicleName, IVehicle vehicle, string fileName, string filePath);
	}

	public class JsonLogger : IJsonLogger
	{
		IFilePerVehicleLogger _filePerVehicleLogger;
		IConsoleManager _consoleManager;

		public JsonLogger(IFilePerVehicleLogger filePerVehicleLogger, IConsoleManager consoleManager)
		{
			_filePerVehicleLogger = filePerVehicleLogger;
			_consoleManager = consoleManager;
		}

		public void CreateJsonFile(ConcurrentDictionary<string, string> localFileChanges, string vehicleName, IVehicle vehicle, string fileName, string filePath)
		{
			LocalWikiFileTypeEnum fileType = LocalWikiFileTypeEnum.JSON;
			string fileExtension = fileType.ToString();

			GroundVehicle groundVehicle = (GroundVehicle)vehicle;
			string vehicleJson = Newtonsoft.Json.JsonConvert.SerializeObject(groundVehicle, Newtonsoft.Json.Formatting.Indented);

			if (!File.Exists(filePath))
			{
				// Add new item
				File.WriteAllText(filePath, vehicleJson);
				_filePerVehicleLogger.RecordAddFileToLocalWiki(localFileChanges, vehicleName, fileName, fileExtension);
			}
			else
			{
				string existingFileText = File.ReadAllText(filePath);
				GroundVehicle existingVehicle = Newtonsoft.Json.JsonConvert.DeserializeObject<GroundVehicle>(existingFileText);

				// Get out the last modified times for comparison
				string newLastModSection = groundVehicle.LastModified;
				string oldLastModSection = existingVehicle?.LastModified;

				// If both files have a last modified time
				if (newLastModSection != null && oldLastModSection != null)
				{
					if (!_filePerVehicleLogger.AreLastModifiedTimesTheSame(oldLastModSection, newLastModSection))
					{
						// Update existing
						File.WriteAllText(filePath, vehicleJson);
						_filePerVehicleLogger.RecordUpdateFileInLocalWiki(localFileChanges, vehicleName, fileName, fileExtension);
					}
				}
				// Add the item if the existing one has no last modified time
				else if (oldLastModSection == null)
				{
					// Update existing item
					File.WriteAllText(filePath, vehicleJson);
					_filePerVehicleLogger.RecordUpdateFileInLocalWiki(localFileChanges, vehicleName, fileName, fileExtension);
				}
				else
				{
					string noLastModifiedSectionExceptionMessage = $"Unable to find the '{ConfigurationManager.AppSettings["LastModifiedSectionId"]}' section, information comparision failed.";

					_consoleManager.WriteException(noLastModifiedSectionExceptionMessage);
					throw new InvalidOperationException(noLastModifiedSectionExceptionMessage);
				}
			}
		}
	}
}