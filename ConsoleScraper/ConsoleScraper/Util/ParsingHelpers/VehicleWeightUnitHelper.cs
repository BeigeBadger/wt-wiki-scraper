using ConsoleScraper.Enums;
using ConsoleScraper.Models.Interfaces;
using ConsoleScraper.Util.Interfaces;

namespace ConsoleScraper.Util.ParsingHelpers
{
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