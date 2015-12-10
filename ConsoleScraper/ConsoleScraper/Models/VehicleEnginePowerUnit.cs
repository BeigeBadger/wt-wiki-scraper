using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicleEnginePowerUnit
	{
		#region Properties
		int Id { get; set; }
		VehicleEnginePowerUnitEnum EnginePowerUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }
		#endregion

		#region Methods
		VehicleEnginePowerUnit GetEngineUnitFromAbbreviation(string enginePowerUnitsAbbreviation);
		#endregion
	}

	public class VehicleEnginePowerUnit : IVehicleStatisticalUnit, IVehicleEnginePowerUnit
	{
		public int Id { get; set; }
		public VehicleEnginePowerUnitEnum EnginePowerUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleEnginePowerUnit() { }

		private VehicleEnginePowerUnit(VehicleEnginePowerUnitEnum unitEnum, string name, string abbreviation)
		{
			EnginePowerUnit = unitEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleEnginePowerUnit GetEngineUnitFromAbbreviation(string enginePowerUnitsAbbreviation)
		{
			if (enginePowerUnitsAbbreviation.Equals("kg"))
			{
				return new VehicleEnginePowerUnit(VehicleEnginePowerUnitEnum.Horsepower, VehicleEnginePowerUnitEnum.Horsepower.ToString(), enginePowerUnitsAbbreviation);
			}

			return new VehicleEnginePowerUnit();
		}
	}
}