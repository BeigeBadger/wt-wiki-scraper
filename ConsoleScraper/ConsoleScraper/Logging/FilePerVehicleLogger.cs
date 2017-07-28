using ConsoleScraper.Logging.Interfaces;
using System.Collections.Concurrent;
using ConsoleScraper.Util.Interfaces;

namespace ConsoleScraper.Logging
{
	public class FilePerVehicleLogger : IFilePerVehicleLogger
	{
		private readonly IConsoleManager _consoleManager;

		public FilePerVehicleLogger(IConsoleManager consoleManager)
		{
			_consoleManager = consoleManager;
		}

		public bool AreLastModifiedTimesTheSame(string oldLastModifiedSection, string newLastModifiedSection)
		{
			return newLastModifiedSection == oldLastModifiedSection;
		}

		public void RecordAddFileToLocalWiki(ConcurrentDictionary<string, string> localFileChanges, string vehicleName, string fileName, string fileType)
		{
			// Record addition of new item
			localFileChanges.TryAdd($"{vehicleName}: {fileType}", $"New vehicle '{fileName}' {fileType} file added to local wiki");
			_consoleManager.WriteTextLine($"New vehicle '{fileName}' {fileType} file added to local wiki");
		}

		public void RecordUpdateFileInLocalWiki(ConcurrentDictionary<string, string> localFileChanges, string vehicleName, string fileName, string fileType)
		{
			// Record update of existing item
			localFileChanges.TryAdd($"{vehicleName}: {fileType}", $"Vehicle '{fileName}' {fileType} file updated in local wiki");
			_consoleManager.WriteTextLine($"Vehicle '{fileName}' {fileType} file updated in local wiki");
		}
	}
}