using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicleWeightUnit
	{
		#region Properties
		int Id { get; set; }
		VehicleWeightUnitEnum WeightUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }
		#endregion

		#region Methods
		VehicleWeightUnit GetWeightUnitFromAbbreviation(string weightUnitsAbbreviation);
		#endregion
	}

	public class VehicleWeightUnit : IVehicleStatisticalUnit, IVehicleWeightUnit
	{
		public int Id { get; set; }
		public VehicleWeightUnitEnum WeightUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleWeightUnit() {}

		private VehicleWeightUnit(VehicleWeightUnitEnum unitEnum, string name, string abbreviation)
		{
			WeightUnit = unitEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleWeightUnit GetWeightUnitFromAbbreviation(string weightUnitsAbbreviation)
		{
			if(weightUnitsAbbreviation.Equals("kg"))
			{
				return new VehicleWeightUnit(VehicleWeightUnitEnum.Kilograms, VehicleWeightUnitEnum.Kilograms.ToString(), weightUnitsAbbreviation);
			}

			return new VehicleWeightUnit();
		}
	}
}