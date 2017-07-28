using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using ConsoleScraper.Util.Interfaces;

namespace ConsoleScraper.Util.Crawlers
{
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
				vehiclePageLinks.TryRemove(vehiclePageLink.Key, out HtmlNode _);

				// Fetch page information
				HtmlNode linkNode = vehiclePageLink.Value;
				string wikiRelativeUrl = linkNode.Attributes.Single(l => l.Name == "href").Value;
				string vehicleWikiEntryFullUrl = new Uri(new Uri(ConfigurationManager.AppSettings["BaseWikiUrl"]), wikiRelativeUrl).ToString();
				string vehicleName = linkNode.InnerText;

				// Write out the vehicle name and url
				_consoleManager.WriteBlankLine();
				_consoleManager.WriteTextLine($"Attempting to scrape information for... {vehicleName}");
				_consoleManager.WriteTextLine($"Vehicle: {vehicleName}");
				_consoleManager.WriteTextLine($"Url: {vehicleWikiEntryFullUrl}");

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