using ConsoleScraper.Enums;
using ConsoleScraper.Util.ParsingHelpers;

namespace ConsoleScraper.Util.Interfaces
{
	public interface IVehicleSpeedUnitHelper
	{
		#region Properties

		int Id { get; set; }
		VehicleSpeedUnitEnum SpeedUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }

		#endregion Properties

		#region Methods

		VehicleSpeedUnitHelper GetSpeedUnitFromAbbreviation(string maxSpeedUnitAbbreviation);

		#endregion Methods
	}
}