using System.Collections.Concurrent;

namespace ConsoleScraper
{
	public interface IFilePerVehicleLogger
	{
		/// <summary>
		/// Returns whether or not the two timestamps from the last modified section for a vehicle match
		/// </summary>
		/// <param name="oldLastModifiedSection">Timestamp for the older file</param>
		/// <param name="newLastModifiedSection">Timestamp for the newer file</param>
		/// <returns>Whether or not the timestamps match</returns>
		bool AreLastModifiedTimesTheSame(string oldLastModifiedSection, string newLastModifiedSection);

		/// <summary>
		/// Records the addition of a file to the local wiki
		/// </summary>
		/// <param name="localFileChanges">The dictionary that holds all of the local changes</param>
		/// <param name="vehicleName">Vehicle the file is for</param>
		/// <param name="fileName">File name that was added</param>
		/// <param name="fileType">File type that was added</param>
		void RecordAddFileToLocalWiki(ConcurrentDictionary<string, string> localFileChanges, string vehicleName, string fileName, string fileType);

		/// <summary>
		/// Records the update of a file in the local wiki
		/// </summary>
		/// <param name="localFileChanges">The dictionary that holds all of the local changes</param>
		/// <param name="vehicleName">Vehicle the file is for</param>
		/// <param name="fileName">File name that was updated</param>
		/// <param name="fileType">File type that was updated</param>
		void RecordUpdateFileInLocalWiki(ConcurrentDictionary<string, string> localFileChanges, string vehicleName, string fileName, string fileType);
	}

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