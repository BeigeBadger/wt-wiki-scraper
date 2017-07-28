namespace ConsoleScraper.Util.Interfaces
{
	public interface IStringHelper
	{
		/// <summary>
		/// Removed invalid filename characters from the provided string
		/// </summary>
		/// <param name="dirtyString">The string which could potentially have invalid characters in it</param>
		/// <returns>A string which is valid for file system pathing</returns>
		string RemoveInvalidCharacters(string dirtyString);
	}
}