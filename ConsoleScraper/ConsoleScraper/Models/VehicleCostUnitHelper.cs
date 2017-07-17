using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
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