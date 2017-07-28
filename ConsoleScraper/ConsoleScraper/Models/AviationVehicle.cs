using ConsoleScraper.Enums;
using ConsoleScraper.Models.Interfaces;
using ConsoleScraper.Util.ParsingHelpers;
using System;

namespace ConsoleScraper.Models
{
	public class AviationVehicle : Vehicle, IAviationVehicle
	{
		// TODO: Needs reference to Vehicle
		public AviationVehicleTypeEnum AviationVehicleType { get; set; }

		public int MaxAltitude { get; set; }
		public double TurnTime { get; set; }
		public int TakeOffDistance { get; set; }
		public string ClimbTime { get; set; }
		public double ClimbRate { get; set; }
		public TimeSpan TimeForFreeRepair { get; set; }
		public VehicleEnginePowerUnitHelper EnginePowerUnit { get; set; }
	}
}