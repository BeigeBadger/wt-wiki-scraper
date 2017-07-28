using System.Collections.Concurrent;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace ConsoleScraper.Util.Interfaces
{
	public interface IWebCrawler
	{
		/// <summary>
		/// Extracts the key, value pairs from the info box and store them
		/// so that they can be used later on to populate vehicle details
		/// </summary>
		/// <param name="vehicleAttributes">The dictionary to store the key, value pairs in</param>
		/// <param name="rows">The table rows to process and extract the key, value pairs from</param>
		void GetAttributesFromInfoBox(Dictionary<string, string> vehicleAttributes, HtmlNodeCollection rows);

		/// <summary>
		/// Finds all of the links to vehicles on the current page, then checks to see
		/// if there is a link to the next page, if there is then that page is loaded
		/// and this method is called recursively until all of the links have been
		/// gathered.
		/// </summary>
		/// <param name="vehicleWikiEntryLinks">The list to store the found links in</param>
		/// <param name="pageUrl">The url of the page to check for more links</param>
		/// <returns>A dictionary that holds the number of links found via dom traversal, and the expected number using the page text</returns>
		Dictionary<string, int> GetLinksToVehiclePages(List<HtmlNode> vehicleWikiEntryLinks, HtmlDocument pageUrl);

		/// <summary>
		/// This is called inside the worker tasks to crawl the pages asynchronously
		/// </summary>
		/// <param name="vehiclePageLinks">A dictionary that contains a indexer for the key, and a link to the wiki page for that vehicle as the value</param>
		/// <param name="vehicleWikiPagesContent">A dictionary that contains the vehicle name as the key and the HTML content of the page as the value</param>
		void GetPageHtml(ConcurrentDictionary<int, HtmlNode> vehiclePageLinks, ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent);

		/// <summary>
		/// Gets the page using the provided URL
		/// </summary>
		/// <param name="url">URL to visit</param>
		/// <returns>An HtmlDocument that represents the page for a given URL</returns>
		HtmlDocument GetDocumentViaUrl(string url);

		/// <summary>
		/// Checks if there were any errors parsing an HtmlDocument
		/// </summary>
		/// <param name="document">The document to check for errors</param>
		/// <returns>Whether there were errors parsing the provided document</returns>
		bool DoesTheDocumentContainParseErrors(HtmlDocument document);
	}
}