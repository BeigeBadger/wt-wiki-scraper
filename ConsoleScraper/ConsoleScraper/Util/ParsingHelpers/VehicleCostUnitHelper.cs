using ConsoleScraper.Enums;
using ConsoleScraper.Models.Interfaces;
using ConsoleScraper.Util.Interfaces;

namespace ConsoleScraper.Util.ParsingHelpers
{
	public class VehicleCostUnitHelper : IVehicleStatisticalUnit, IVehicleCostUnitHelper
	{
		public int Id { get; set; }
		public VehicleCostUnitEnum CostUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleCostUnitHelper()
		{
		}

		private VehicleCostUnitHelper(VehicleCostUnitEnum unitEnum, string name, string abbreviation)
		{
			Id = (int)unitEnum;
			CostUnit = unitEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleCostUnitHelper GetCostUnitFromAbbreviation(string maxRepairCostUnitsAbbreviation)
		{
			if (maxRepairCostUnitsAbbreviation.Equals("s.l."))
			{
				return new VehicleCostUnitHelper(VehicleCostUnitEnum.SilverLions, VehicleCostUnitEnum.SilverLions.ToString(), maxRepairCostUnitsAbbreviation);
			}
			// TODO: Add support for Golden Eagles, but these aren't used on the wiki at present, the cost just shows as 0 s.l.

			return new VehicleCostUnitHelper();
		}
	}
}