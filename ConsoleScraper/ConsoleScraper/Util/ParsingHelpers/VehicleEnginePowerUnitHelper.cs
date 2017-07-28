using ConsoleScraper.Enums;
using ConsoleScraper.Models.Interfaces;
using ConsoleScraper.Util.Interfaces;

namespace ConsoleScraper.Util.ParsingHelpers
{
	public class VehicleEnginePowerUnitHelper : IVehicleStatisticalUnit, IVehicleEnginePowerUnitHelper
	{
		public int Id { get; set; }
		public VehicleEnginePowerUnitEnum EnginePowerUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleEnginePowerUnitHelper()
		{
		}

		private VehicleEnginePowerUnitHelper(VehicleEnginePowerUnitEnum unitEnum, string name, string abbreviation)
		{
			Id = (int)unitEnum;
			EnginePowerUnit = unitEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleEnginePowerUnitHelper GetEngineUnitFromAbbreviation(string enginePowerUnitsAbbreviation)
		{
			if (enginePowerUnitsAbbreviation.Equals("h.p.") || enginePowerUnitsAbbreviation.Equals("hp"))
			{
				return new VehicleEnginePowerUnitHelper(VehicleEnginePowerUnitEnum.Horsepower, VehicleEnginePowerUnitEnum.Horsepower.ToString(), enginePowerUnitsAbbreviation);
			}

			return new VehicleEnginePowerUnitHelper();
		}
	}
}