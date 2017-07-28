using ConsoleScraper.Util.Interfaces;
using System.IO;
using System.Linq;

namespace ConsoleScraper.Util
{
	public class StringHelper : IStringHelper
	{
		public StringHelper()
		{
		}

		public string RemoveInvalidCharacters(string dirtyString)
		{
			var invalidChars = Path.GetInvalidFileNameChars();

			return new string(dirtyString
				.Where(x => !invalidChars.Contains(x))
				.ToArray()
			);
		}
	}
}