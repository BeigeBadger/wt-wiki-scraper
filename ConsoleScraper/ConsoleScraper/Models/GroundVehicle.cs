using System;
using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
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
		#endregion
	}

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