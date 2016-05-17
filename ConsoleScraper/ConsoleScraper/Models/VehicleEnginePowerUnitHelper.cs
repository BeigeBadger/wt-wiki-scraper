using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicleEnginePowerUnitHelper
	{
		#region Properties
		int Id { get; set; }
		VehicleEnginePowerUnitEnum EnginePowerUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }
		#endregion

		#region Methods
		VehicleEnginePowerUnitHelper GetEngineUnitFromAbbreviation(string enginePowerUnitsAbbreviation);
		#endregion
	}

	public class VehicleEnginePowerUnitHelper : IVehicleStatisticalUnit, IVehicleEnginePowerUnitHelper
	{
		public int Id { get; set; }
		public VehicleEnginePowerUnitEnum EnginePowerUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleEnginePowerUnitHelper() { }

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