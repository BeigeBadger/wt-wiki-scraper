using ConsoleScraper.Enums;
using ConsoleScraper.Util.ParsingHelpers;

namespace ConsoleScraper.Util.Interfaces
{
	public interface IVehicleWeightUnitHelper
	{
		#region Properties

		int Id { get; set; }
		VehicleWeightUnitEnum WeightUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }

		#endregion Properties

		#region Methods

		VehicleWeightUnitHelper GetWeightUnitFromAbbreviation(string weightUnitsAbbreviation);

		#endregion Methods
	}
}