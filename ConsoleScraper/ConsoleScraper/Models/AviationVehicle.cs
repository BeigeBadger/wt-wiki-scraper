using System;
using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IAviationVehicle : IVehicle
	{
		#region Properties

		AviationVehicleTypeEnum AviationVehicleType { get; set; }

		int MaxAltitude { get; set; }
		double TurnTime { get; set; }
		int TakeOffDistance { get; set; }
		string ClimbTime { get; set; }
		double ClimbRate { get; set; }
		TimeSpan TimeForFreeRepair { get; set; }
		VehicleEnginePowerUnitHelper EnginePowerUnit { get; set; }

		#endregion Properties
	}

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