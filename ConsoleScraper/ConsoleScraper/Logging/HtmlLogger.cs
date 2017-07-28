using ConsoleScraper.Enums;
using ConsoleScraper.Logging.Interfaces;
using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleScraper.Util.Interfaces;

namespace ConsoleScraper.Logging
{
	public class HtmlLogger : IHtmlLogger
	{
		private readonly IFilePerVehicleLogger _filePerVehicleLogger;
		private readonly IConsoleManager _consoleManager;

		public HtmlLogger(IFilePerVehicleLogger filePerVehicleLogger, IConsoleManager consoleManager)
		{
			_filePerVehicleLogger = filePerVehicleLogger;
			_consoleManager = consoleManager;
		}

		public void CreateHtmlFile(ConcurrentDictionary<string, string> localFileChanges, HtmlDocument vehicleWikiPage, string vehicleName, string fileName, string filePath)
		{
			LocalWikiFileTypeEnum fileType = LocalWikiFileTypeEnum.Html;
			string fileExtension = fileType.ToString();

			if (!File.Exists(filePath))
			{
				// Add new item
				vehicleWikiPage.Save($"{filePath}", Encoding.UTF8);
				_filePerVehicleLogger.RecordAddFileToLocalWiki(localFileChanges, vehicleName, fileName, fileExtension);
			}
			else
			{
				string existingFileText = File.ReadAllText(filePath);

				//Create a fake document so we can use helper methods to traverse through the existing file as an HTML document
				HtmlDocument htmlDoc = new HtmlDocument();
				HtmlNode existingHtml = HtmlNode.CreateNode(existingFileText);
				htmlDoc.DocumentNode.AppendChild(existingHtml);

				// Get out the last modified times for comparison
				var newLastModSection = vehicleWikiPage.DocumentNode.Descendants().SingleOrDefault(x => x.Id == ConfigurationManager.AppSettings["LastModifiedSectionId"]);
				var oldLastModSection = existingHtml.OwnerDocument.DocumentNode.Descendants().SingleOrDefault(x => x.Id == ConfigurationManager.AppSettings["LastModifiedSectionId"]);

				// If both files have a last modified time
				if (newLastModSection != null && oldLastModSection != null)
				{
					// Update the existing one if the times are different
					if (!_filePerVehicleLogger.AreLastModifiedTimesTheSame(oldLastModSection.InnerHtml, newLastModSection.InnerHtml))
					{
						// Update existing item
						vehicleWikiPage.Save($"{filePath}", Encoding.UTF8);
						_filePerVehicleLogger.RecordUpdateFileInLocalWiki(localFileChanges, vehicleName, fileName, fileExtension);
					}
				}
				// Add the item if the existing one has no last modified time
				else if (oldLastModSection == null)
				{
					// Update existing item
					vehicleWikiPage.Save($"{filePath}", Encoding.UTF8);
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