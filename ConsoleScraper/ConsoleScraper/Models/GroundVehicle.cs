using System;
using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IGroundVehicle
	{
		#region Properties
		string Name { get; set; }
		CountryEnum Country { get; set; }
		int Weight { get; set; }
		GroundVehicleTypeHelper VehicleType { get; set; }
		int Rank { get; set; }
		double BattleRating { get; set; }
		int EnginePower { get; set; }
		double MaxSpeed { get; set; }
		string HullArmourThickness { get; set; }
		string SuperStructureArmourThickness { get; set; }
		TimeSpan TimeForFreeRepair { get; set; }
		long MaxRepairCost { get; set; }
		long PurchaseCost { get; set; }
		VehicleCostUnitHelper PurchaseCostUnit { get; set; }
		VehicleCostUnitHelper MaxRepairCostUnit { get; set; }
		VehicleSpeedUnitHelper MaxSpeedUnit { get; set; }
		VehicleWeightUnitHelper WeightUnit { get; set; }
		VehicleEnginePowerUnitHelper EnginePowerUnit { get; set; }
		#endregion
	}

	public class GroundVehicle : IVehicle, IGroundVehicle
	{
		public string Name { get; set; }
		public CountryEnum Country { get; set; }
		public int Weight { get; set; }
		public GroundVehicleTypeHelper VehicleType { get; set; }
		public int Rank { get; set; }
		public double BattleRating { get; set; }
		public int EnginePower { get; set; }
		public double MaxSpeed { get; set; }
		public string HullArmourThickness { get; set; }
		public string SuperStructureArmourThickness { get; set; }
		public TimeSpan TimeForFreeRepair { get; set; }
		public long MaxRepairCost { get; set; }
		public long PurchaseCost { get; set; }
		public VehicleCostUnitHelper PurchaseCostUnit { get; set; }
		public VehicleCostUnitHelper MaxRepairCostUnit { get; set; }
		public VehicleSpeedUnitHelper MaxSpeedUnit { get; set; }
		public VehicleWeightUnitHelper WeightUnit { get; set; }
		public VehicleEnginePowerUnitHelper EnginePowerUnit { get; set; }
	}
}