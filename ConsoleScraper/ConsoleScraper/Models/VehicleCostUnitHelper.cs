﻿using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicleCostUnitHelper
	{
		#region Properties
		int Id { get; set; }
		VehicleCostUnitEnum CostUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }
		#endregion

		#region Methods
		VehicleCostUnitHelper GetCostUnitFromAbbreviation(string maxRepairCostUnitsAbbreviation);
		#endregion
	}

	public class VehicleCostUnitHelper : IVehicleStatisticalUnit, IVehicleCostUnitHelper
	{
		public int Id { get; set; }
		public VehicleCostUnitEnum CostUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleCostUnitHelper() { }

		public VehicleCostUnitHelper(VehicleCostUnitEnum unitEnum, string name, string abbreviation)
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

			return new VehicleCostUnitHelper();
		}
	}
}