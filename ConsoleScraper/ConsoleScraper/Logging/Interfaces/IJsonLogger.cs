using ConsoleScraper.Models.Interfaces;
using System.Collections.Concurrent;

namespace ConsoleScraper.Logging.Interfaces
{
	public interface IJsonLogger
	{
		/// <summary>
		/// Creates a JSON file in the local wiki using info
		/// scraped from the wiki entry
		/// </summary>
		/// <param name="localFileChanges">The dictionary that holds all of the local changes</param>
		/// <param name="vehicleName">Name of the vehicle the page is for</param>
		/// <param name="vehicle">The vehicle to update</param>
		/// <param name="fileName">What the file should be called</param>
		/// <param name="filePath">Where the file should be stored</param>
		void CreateJsonFile(ConcurrentDictionary<string, string> localFileChanges, string vehicleName, IVehicle vehicle, string fileName, string filePath);
	}
}