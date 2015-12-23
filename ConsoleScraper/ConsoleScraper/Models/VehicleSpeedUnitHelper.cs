﻿using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicleSpeedUnitHelper
	{
		#region Properties
		int Id { get; set; }
		VehicleSpeedUnitEnum SpeedUnit { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }
		#endregion

		#region Methods
		VehicleSpeedUnitHelper GetSpeedUnitFromAbbreviation(string maxSpeedUnitAbbreviation);
		#endregion
	}

	public class VehicleSpeedUnitHelper : IVehicleStatisticalUnit, IVehicleSpeedUnitHelper
	{
		public int Id { get; set; }
		public VehicleSpeedUnitEnum SpeedUnit { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public VehicleSpeedUnitHelper() { }

		public VehicleSpeedUnitHelper(VehicleSpeedUnitEnum unitEnum, string name, string abbreviation)
		{
			Id = (int)unitEnum;
			SpeedUnit = unitEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleSpeedUnitHelper GetSpeedUnitFromAbbreviation(string maxSpeedUnitAbbreviation)
		{
			if (maxSpeedUnitAbbreviation.Equals("km/h"))
			{
				return new VehicleSpeedUnitHelper(VehicleSpeedUnitEnum.KilometersPerHour, VehicleSpeedUnitEnum.KilometersPerHour.ToString(), maxSpeedUnitAbbreviation);
			}

			return new VehicleSpeedUnitHelper();
		}
	}
}