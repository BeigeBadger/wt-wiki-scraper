using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleScraper
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

	public class WebCrawler : IWebCrawler
	{
		private readonly IConsoleManager _consoleManager;

		private int _totalNumberOfLinksBasedOnPageText;
		private int _totalNumberOfLinksFoundViaDomTraversal;

		public WebCrawler(IConsoleManager consoleManager)
		{
			_consoleManager = consoleManager;

			_totalNumberOfLinksBasedOnPageText = 0;
			_totalNumberOfLinksFoundViaDomTraversal = 0;
		}

		public void GetAttributesFromInfoBox(Dictionary<string, string> vehicleAttributes, HtmlNodeCollection rows)
		{
			// Traverse the info box and pull out all of the attribute title and value pairs
			foreach (HtmlNode row in rows)
			{
				HtmlNodeCollection cells = row.SelectNodes("td");

				// Get the property name and value and add them to the dictionary before writing them out
				string rowTitle = cells.First().SelectNodes("b").Single().InnerText.Trim();
				string rowValue = cells.Last().InnerText.Trim();

				vehicleAttributes.Add(rowTitle, rowValue);

				_consoleManager.WriteLineInColour(ConsoleColor.DarkGreen, $"{rowTitle}: {rowValue}");
			}
		}

		public Dictionary<string, int> GetLinksToVehiclePages(List<HtmlNode> vehicleWikiEntryLinks, HtmlDocument pageUrl)
		{
			// Get "Pages in category "Ground vehicles"" section | <div id="mw-pages"> | document.getElementById('mw-pages')
			HtmlNode listContainerNode = pageUrl.DocumentNode.Descendants().Single(d => d.Id == "mw-pages");
			// Get container that holds the table with the links | <div lang="en" dir="ltr" class="mw-content-ltr"> | document.getElementsByClassName('mw-content-ltr')[1]
			HtmlNode tableContainerNode = listContainerNode.Descendants("div").Single(d => d.Attributes["class"].Value.Contains("mw-content-ltr"));

			// Get Vehicle links from the initial page | div > table > tbody > tr > td > ul > li > a | document.getElementsByClassName('mw-content-ltr')[1].getElementsByTagName('a')
			vehicleWikiEntryLinks.AddRange(tableContainerNode.Descendants("table").Single().Descendants("a").ToList());

			// Get totals for the number of links to expect, and the number found
			string totalEntriesTextBlock = listContainerNode.Descendants("p").Single().InnerText;
			MatchCollection matches = Regex.Matches(totalEntriesTextBlock, @"\d+");
			_totalNumberOfLinksBasedOnPageText = int.Parse(matches[matches.Count - 1].Value);
			_totalNumberOfLinksFoundViaDomTraversal = vehicleWikiEntryLinks.Count;

			// Get vehicle links from the subsequent pages | <a href="/index.php?title=Category:Ground_vehicles&amp;pagefrom=T-54+mod.+1949#mw-pages" title="Category:Ground vehicles">next 200</a> | document.querySelectorAll('#mw-pages a[Title="Category:Ground vehicles"]')[0]
			HtmlNode nextPageLink = listContainerNode.Descendants("a").FirstOrDefault(d => d.InnerText.Contains("next") && d.Attributes["title"].Value.Contains("Category:Ground vehicles"));

			if (nextPageLink != null)
			{
				// Build the link for the next page
				Uri subsequentWikPage = new Uri(new Uri(ConfigurationManager.AppSettings["BaseWikiUrl"]), nextPageLink.Attributes["href"].Value);
				string subsequentPageUrl = System.Net.WebUtility.HtmlDecode(subsequentWikPage.ToString());

				// Load Wiki page
				HtmlWeb webGet = new HtmlWeb();
				HtmlDocument groundForcesWikiPage = webGet.Load(subsequentPageUrl);

				// Recursively call this method until we've got all of the vehicles
				GetLinksToVehiclePages(vehicleWikiEntryLinks, groundForcesWikiPage);
			}
			else
			{
				_consoleManager.WriteBlankLine();
				_consoleManager.WriteLineInColour(ConsoleColor.Green, "Finished retrieving links to vehicle pages.");
			}

			return new Dictionary<string, int> {
				{ "TotalNumberOfLinksBasedOnPageText", _totalNumberOfLinksBasedOnPageText },
				{ "TotalNumberOfLinksFoundViaDomTraversal", _totalNumberOfLinksFoundViaDomTraversal }
			};
		}

		public void GetPageHtml(ConcurrentDictionary<int, HtmlNode> vehiclePageLinks, ConcurrentDictionary<string, HtmlDocument> vehicleWikiPagesContent)
		{
			foreach (var vehiclePageLink in vehiclePageLinks)
			{
				// Remove the current node so that the other threads don't reprocess it
				HtmlNode tempNode;
				vehiclePageLinks.TryRemove(vehiclePageLink.Key, out tempNode);

				// Fetch page information
				HtmlNode linkNode = vehiclePageLink.Value;
				string wikiRelativeUrl = linkNode.Attributes.Single(l => l.Name == "href").Value;
				string vehicleWikiEntryFullUrl = new Uri(new Uri(ConfigurationManager.AppSettings["BaseWikiUrl"]), wikiRelativeUrl).ToString();
				string vehicleName = linkNode.InnerText;

				// Write out the vehicle name and url
				_consoleManager.WriteBlankLine();
				_consoleManager.WriteTextLine($"Processing... {vehicleName}");
				_consoleManager.WriteTextLine($"Vehicle: {vehicleName}, Url: {vehicleWikiEntryFullUrl}");

				// Visit page and extract data
				HtmlWeb vehicleWebGet = new HtmlWeb();
				HtmlDocument vehicleWikiPage = vehicleWebGet.Load(vehicleWikiEntryFullUrl);

				// Add page to new dictionary used to extract further data
				vehicleWikiPagesContent.TryAdd(vehicleName, vehicleWikiPage);

				_consoleManager.WriteTextLine(vehicleWikiPagesContent.Count.ToString());
			}
		}

		public HtmlDocument GetDocumentViaUrl(string url)
		{
			HtmlWeb webGet = new HtmlWeb();

			// Return the retrieved document
			return webGet.Load(url);
		}

		public bool DoesTheDocumentContainParseErrors(HtmlDocument document)
		{
			return document.ParseErrors != null && document.ParseErrors.Any();
		}
	}
}