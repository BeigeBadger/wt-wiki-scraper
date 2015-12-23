using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicleWeightUnitHelper
	{
		#region Properties
		int Id { get; set; }
		VehicleWeightUnitEnum WeightUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }
		#endregion

		#region Methods
		VehicleWeightUnitHelper GetWeightUnitFromAbbreviation(string weightUnitsAbbreviation);
		#endregion
	}

	public class VehicleWeightUnitHelper : IVehicleStatisticalUnit, IVehicleWeightUnitHelper
	{
		public int Id { get; set; }
		public VehicleWeightUnitEnum WeightUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleWeightUnitHelper() {}

		private VehicleWeightUnitHelper(VehicleWeightUnitEnum unitEnum, string name, string abbreviation)
		{
			Id = (int)unitEnum;
			WeightUnit = unitEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleWeightUnitHelper GetWeightUnitFromAbbreviation(string weightUnitsAbbreviation)
		{
			if(weightUnitsAbbreviation.Equals("kg"))
			{
				return new VehicleWeightUnitHelper(VehicleWeightUnitEnum.Kilograms, VehicleWeightUnitEnum.Kilograms.ToString(), weightUnitsAbbreviation);
			}

			return new VehicleWeightUnitHelper();
		}
	}
}