using ConsoleScraper.Enums;
using ConsoleScraper.Models.Interfaces;
using ConsoleScraper.Util.Interfaces;

namespace ConsoleScraper.Util.ParsingHelpers
{
	public class VehicleSpeedUnitHelper : IVehicleStatisticalUnit, IVehicleSpeedUnitHelper
	{
		public int Id { get; set; }
		public VehicleSpeedUnitEnum SpeedUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleSpeedUnitHelper()
		{
		}

		private VehicleSpeedUnitHelper(VehicleSpeedUnitEnum unitEnum, string name, string abbreviation)
		{
			Id = (int)unitEnum;
			SpeedUnit = unitEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleSpeedUnitHelper GetSpeedUnitFromAbbreviation(string maxSpeedUnitAbbreviation)
		{
			// TODO: Make switch-case
			if (maxSpeedUnitAbbreviation.Equals("km/h"))
			{
				return new VehicleSpeedUnitHelper(VehicleSpeedUnitEnum.KilometersPerHour, VehicleSpeedUnitEnum.KilometersPerHour.ToString(), maxSpeedUnitAbbreviation);
			}
			if (maxSpeedUnitAbbreviation.Equals("mph"))
			{
				return new VehicleSpeedUnitHelper(VehicleSpeedUnitEnum.MilesPerHour, VehicleSpeedUnitEnum.MilesPerHour.ToString(), maxSpeedUnitAbbreviation);
			}

			return new VehicleSpeedUnitHelper();
		}
	}
}