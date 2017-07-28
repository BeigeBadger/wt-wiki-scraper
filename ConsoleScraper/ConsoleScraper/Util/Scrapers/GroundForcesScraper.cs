using HtmlAgilityPack;
using System.Configuration;
using ConsoleScraper.Util.Interfaces;

namespace ConsoleScraper.Util.Scrapers
{
	public class GroundForcesScraper : IGroundForcesScraper
	{
		private readonly IWebCrawler _webCrawler;

		public GroundForcesScraper(IWebCrawler webCrawler)
		{
			_webCrawler = webCrawler;
		}

		public HtmlDocument GetGroundForcesWikiHomePage()
		{
			return _webCrawler.GetDocumentViaUrl(ConfigurationManager.AppSettings["GroundForcesWikiUrl"]);
		}
	}
}