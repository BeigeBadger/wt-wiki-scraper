using ConsoleScraper.Enums;
using ConsoleScraper.Util.ParsingHelpers;

namespace ConsoleScraper.Util.Interfaces
{
	public interface IVehicleCostUnitHelper
	{
		#region Properties

		int Id { get; set; }
		VehicleCostUnitEnum CostUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }

		#endregion Properties

		#region Methods

		VehicleCostUnitHelper GetCostUnitFromAbbreviation(string maxRepairCostUnitsAbbreviation);

		#endregion Methods
	}
}