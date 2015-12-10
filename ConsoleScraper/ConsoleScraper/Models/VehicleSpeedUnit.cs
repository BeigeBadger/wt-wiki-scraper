using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicleSpeedUnit
	{
		#region Properties
		int Id { get; set; }
		VehicleSpeedUnitEnum SpeedUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }
		#endregion

		#region Methods
		VehicleSpeedUnit GetSpeedUnitFromAbbreviation(string maxSpeedUnitAbbreviation);
		#endregion
	}

	public class VehicleSpeedUnit : IVehicleStatisticalUnit, IVehicleSpeedUnit
	{
		public int Id { get; set; }
		public VehicleSpeedUnitEnum SpeedUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleSpeedUnit() { }

		public VehicleSpeedUnit(VehicleSpeedUnitEnum unitEnum, string name, string abbreviation)
		{
			SpeedUnit = unitEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleSpeedUnit GetSpeedUnitFromAbbreviation(string maxSpeedUnitAbbreviation)
		{
			if (maxSpeedUnitAbbreviation.Equals("km/h"))
			{
				return new VehicleSpeedUnit(VehicleSpeedUnitEnum.KilometersPerHour, VehicleSpeedUnitEnum.KilometersPerHour.ToString(), maxSpeedUnitAbbreviation);
			}

			return new VehicleSpeedUnit();
		}
	}
}