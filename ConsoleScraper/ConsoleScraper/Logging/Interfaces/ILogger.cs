using ConsoleScraper.Enums;
using ConsoleScraper.Models.Interfaces;
using HtmlAgilityPack;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ConsoleScraper.Logging.Interfaces
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
}