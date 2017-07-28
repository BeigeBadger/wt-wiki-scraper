using ConsoleScraper.Enums;
using ConsoleScraper.Util.ParsingHelpers;

namespace ConsoleScraper.Util.Interfaces
{
	public interface IVehicleCountryHelper
	{
		#region Properties

		int Id { get; set; }
		CountryEnum CountryEnum { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }

		#endregion Properties

		#region Methods

		VehicleCountryHelper GetVehicleCountryFromAbbreviation(string countryAbbreviation);

		VehicleCountryHelper GetVehicleCountryFromName(string countryName);

		#endregion Methods
	}
}