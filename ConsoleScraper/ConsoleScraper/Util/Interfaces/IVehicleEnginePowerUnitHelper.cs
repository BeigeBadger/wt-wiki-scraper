using ConsoleScraper.Enums;
using ConsoleScraper.Util.ParsingHelpers;

namespace ConsoleScraper.Util.Interfaces
{
	public interface IVehicleEnginePowerUnitHelper
	{
		#region Properties

		int Id { get; set; }
		VehicleEnginePowerUnitEnum EnginePowerUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }

		#endregion Properties

		#region Methods

		VehicleEnginePowerUnitHelper GetEngineUnitFromAbbreviation(string enginePowerUnitsAbbreviation);

		#endregion Methods
	}
}