using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using ConsoleScraper.Models;
using HtmlAgilityPack;

namespace ConsoleScraper.Util.Interfaces
{
	public interface IDataProcessor
	{
		/// <summary>
		/// Finds links to all of the wiki pages for a vehicle type,
		/// then goes through and visits each one, extracting the
		/// data into an HtmlDocument for us in the Process method.
		/// </summary>
		/// <param name="wikiHomePage">The document containing the links to the vehicle wiki pages</param>
		/// <param name="vehicleWikiPagesContent">Holds the HtmlDocuments that are retrieved</param>
		/// <param name="localFileChanges">Holds all of the changes to local files</param>
		/// <param name="vehicleDetails">Holds a mapping between the page title and the vehicle object with the extracted data</param>
		/// <param name="errorsList">Holds the errors that were encountered during processing</param>
		/// <param name="overallStopwatch">Debug only: Holds the stopwatch that is used to record the execution time</param>
		/// <param name="createJsonFiles">Holds whether or not to create Json files as an output</param>
		/// <param name="createHtmlFiles">Holds whether or not to create Html files as an output</param>
		/// <param name="createExcelFile">Holds whether or not to create an Excel file as an output</param>
		void CrawlWikiSectionPagesForData(HtmlDocument wikiHomePage, ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent,
			ConcurrentDictionary<string, string> localFileChanges, Dictionary<string, GroundVehicle> vehicleDetails, List<string> errorsList,
			Stopwatch overallStopwatch, bool createJsonFiles, bool createHtmlFiles, bool createExcelFile);

		/// <summary>
		/// Loops through all of the vehicle wiki links that have been provided, attempts to parse the parts that we are interested in -
		/// the vehicle details table, creates an object from that, then stores the data locally if a flag is set
		/// </summary>
		/// <param name="vehicleWikiPagesContent">Populated with the vehicle name as the key and the HTML content of the page as the value</param>
		/// <param name="localFileChanges">Keeps track of changes made to local files, vehicle name as the key, and the action performed as the value</param>
		/// <param name="vehicleDetails">Populated with the vehicle name and vehicle objects</param>
		/// <param name="vehicleWikiEntryLinks">List that holds the anchor nodes of the relative urls to each vehicle page</param>
		/// <param name="errorsList">Holds any errors that occur during execution</param>
		/// <param name="indexPosition">The current index we are up to processing - used for error messages</param>
		/// <param name="expectedNumberOfLinks">The expected number of links to process</param>
		/// <param name="createJsonFiles">Whether or not to output a JSON file per vehicle</param>
		/// <param name="createHtmlFiles">Whether or not to output a HTML file per vehicle</param>
		/// <param name="createExcelFile">Whether or not to output a Excel file per vehicle</param>
		void ProcessGroundForcesWikiHtmlFiles(ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent, ConcurrentDictionary<string, string> localFileChanges,
			Dictionary<string, GroundVehicle> vehicleDetails, List<HtmlNode> vehicleWikiEntryLinks, List<string> errorsList, int indexPosition, int expectedNumberOfLinks,
			bool createJsonFiles, bool createHtmlFiles, bool createExcelFile);
	}
}