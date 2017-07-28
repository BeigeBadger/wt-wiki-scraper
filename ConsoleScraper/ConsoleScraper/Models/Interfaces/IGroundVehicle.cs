using ConsoleScraper.Util.ParsingHelpers;
using System;

namespace ConsoleScraper.Models.Interfaces
{
	public interface IGroundVehicle : IVehicle
	{
		#region Properties

		int Weight { get; set; }
		GroundVehicleTypeHelper GroundVehicleType { get; set; }
		int EnginePower { get; set; }
		string HullArmourThickness { get; set; }
		string SuperstructureArmourThickness { get; set; }
		TimeSpan TimeForFreeRepair { get; set; }
		VehicleWeightUnitHelper WeightUnit { get; set; }
		VehicleEnginePowerUnitHelper EnginePowerUnit { get; set; }

		#endregion Properties
	}
}