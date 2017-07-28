using ConsoleScraper.Models.Interfaces;
using ConsoleScraper.Util.ParsingHelpers;
using System;

namespace ConsoleScraper.Models
{
	public class GroundVehicle : Vehicle, IGroundVehicle
	{
		// TODO: Needs reference to Vehicle
		public int Weight { get; set; }

		public GroundVehicleTypeHelper GroundVehicleType { get; set; }
		public int EnginePower { get; set; }
		public string HullArmourThickness { get; set; }
		public string SuperstructureArmourThickness { get; set; }
		public TimeSpan TimeForFreeRepair { get; set; }
		public VehicleWeightUnitHelper WeightUnit { get; set; }
		public VehicleEnginePowerUnitHelper EnginePowerUnit { get; set; }
	}
}