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

		#endregion Properties

		#region Methods

		VehicleWeightUnitHelper GetWeightUnitFromAbbreviation(string weightUnitsAbbreviation);

		#endregion Methods
	}

	public class VehicleWeightUnitHelper : IVehicleStatisticalUnit, IVehicleWeightUnitHelper
	{
		public int Id { get; set; }
		public VehicleWeightUnitEnum WeightUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleWeightUnitHelper()
		{
		}

		private VehicleWeightUnitHelper(VehicleWeightUnitEnum unitEnum, string name, string abbreviation)
		{
			Id = (int)unitEnum;
			WeightUnit = unitEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleWeightUnitHelper GetWeightUnitFromAbbreviation(string weightUnitsAbbreviation)
		{
			// TODO: Make switch-case
			if (weightUnitsAbbreviation.Equals("kg"))
			{
				return new VehicleWeightUnitHelper(VehicleWeightUnitEnum.Kilograms, VehicleWeightUnitEnum.Kilograms.ToString(), weightUnitsAbbreviation);
			}
			if (weightUnitsAbbreviation.Equals("lb"))
			{
				return new VehicleWeightUnitHelper(VehicleWeightUnitEnum.Pounds, VehicleWeightUnitEnum.Pounds.ToString(), weightUnitsAbbreviation);
			}

			return new VehicleWeightUnitHelper();
		}
	}
}