using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicleCostUnit
	{
		#region Properties
		int Id { get; set; }
		VehicleCostUnitEnum CostUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }
		#endregion

		#region Methods
		VehicleCostUnit GetCostUnitFromAbbreviation(string maxRepairCostUnitsAbbreviation);
		#endregion
	}

	public class VehicleCostUnit : IVehicleStatisticalUnit, IVehicleCostUnit
	{
		public int Id { get; set; }
		public VehicleCostUnitEnum CostUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleCostUnit() { }

		public VehicleCostUnit(VehicleCostUnitEnum unitEnum, string name, string abbreviation)
		{
			CostUnit = unitEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleCostUnit GetCostUnitFromAbbreviation(string maxRepairCostUnitsAbbreviation)
		{
			if (maxRepairCostUnitsAbbreviation.Equals("s.l."))
			{
				return new VehicleCostUnit(VehicleCostUnitEnum.SilverLions, VehicleCostUnitEnum.SilverLions.ToString(), maxRepairCostUnitsAbbreviation);
			}

			return new VehicleCostUnit();
		}
	}
}