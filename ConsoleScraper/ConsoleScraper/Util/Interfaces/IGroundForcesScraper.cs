using HtmlAgilityPack;

namespace ConsoleScraper.Util.Interfaces
{
	public interface IGroundForcesScraper
	{
		/// <summary>
		/// Gets the HtmlDocument representation of the ground forces wiki homepage
		/// </summary>
		/// <returns>The home page for the ground forces wiki as an HtmlDocument</returns>
		HtmlDocument GetGroundForcesWikiHomePage();
	}
}