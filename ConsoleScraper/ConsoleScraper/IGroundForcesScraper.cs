using HtmlAgilityPack;
using System.Configuration;

namespace ConsoleScraper
{
	public interface IGroundForcesScraper
	{
		/// <summary>
		/// Gets the HtmlDocument representation of the ground forces wiki homepage
		/// </summary>
		/// <returns>The home page for the ground forces wiki as an HtmlDocument</returns>
		HtmlDocument GetGroundForcesWikiHomePage();
	}

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