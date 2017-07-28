using ConsoleScraper.Enums;
using ConsoleScraper.Util.ParsingHelpers;
using System;

namespace ConsoleScraper.Models.Interfaces
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
}