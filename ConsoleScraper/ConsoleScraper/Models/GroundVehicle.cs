using System;
using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IGroundVehicle
	{
		#region Properties
		string Title { get; set; }
		CountryEnum Country { get; set; }
		int Weight { get; set; }
		GroundVehicleTypeEnum VehicleType { get; set; }
		int Rank { get; set; }
		double BattleRating { get; set; }
		int EnginePower { get; set; }
		double MaxSpeed { get; set; }
		string HullArmourThickness { get; set; }
		string SuperStructureArmourThickness { get; set; }
		TimeSpan TimeForFreeRepair { get; set; }
		long MaxRepairCost { get; set; }
		long Cost { get; set; }
		VehicleCostUnit PurchaseCostUnit { get; set; }
		VehicleCostUnit MaxRepairCostUnit { get; set; }
		VehicleSpeedUnit MaxSpeedUnit { get; set; }
		VehicleWeightUnit WeightUnit { get; set; }
		VehicleEnginePowerUnit EnginePowerUnit { get; set; }
		#endregion
	}

	public class GroundVehicle : IVehicle, IGroundVehicle
	{
		public string Title { get; set; }
		public CountryEnum Country { get; set; }
		public int Weight { get; set; }
		public GroundVehicleTypeEnum VehicleType { get; set; }
		public int Rank { get; set; }
		public double BattleRating { get; set; }
		public int EnginePower { get; set; }
		public double MaxSpeed { get; set; }
		public string HullArmourThickness { get; set; }
		public string SuperStructureArmourThickness { get; set; }
		public TimeSpan TimeForFreeRepair { get; set; }
		public long MaxRepairCost { get; set; }
		public long Cost { get; set; }
		public VehicleCostUnit PurchaseCostUnit { get; set; }
		public VehicleCostUnit MaxRepairCostUnit { get; set; }
		public VehicleSpeedUnit MaxSpeedUnit { get; set; }
		public VehicleWeightUnit WeightUnit { get; set; }
		public VehicleEnginePowerUnit EnginePowerUnit { get; set; }
	}
}