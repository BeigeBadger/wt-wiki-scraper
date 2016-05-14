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
		private IWebCrawler _webCrawler;
		private IConsoleManager _consoleManager;

		public GroundForcesScraper(IWebCrawler webCrawler, IConsoleManager consoleManager)
		{
			_webCrawler = webCrawler;
			_consoleManager = consoleManager;
		}

		public HtmlDocument GetGroundForcesWikiHomePage()
		{
			return _webCrawler.GetDocumentViaUrl(ConfigurationManager.AppSettings["GroundForcesWikiUrl"]);
		}
	}
}