﻿using HtmlAgilityPack;
using System.Collections.Concurrent;

namespace ConsoleScraper.Logging.Interfaces
{
	public interface IHtmlLogger
	{
		/// <summary>
		/// Creates an HTML file in the local wiki using info
		/// scraped from the wiki entry
		/// </summary>
		/// <param name="localFileChanges">The dictionary that holds all of the local changes</param>
		/// <param name="vehicleWikiPage">HTML document that represents the wiki page</param>
		/// <param name="vehicleName">Name of the vehicle the page is for</param>
		/// <param name="fileName">What the file should be called</param>
		/// <param name="filePath">Where the file should be stored</param>
		void CreateHtmlFile(ConcurrentDictionary<string, string> localFileChanges, HtmlDocument vehicleWikiPage, string vehicleName, string fileName, string filePath);
	}
}