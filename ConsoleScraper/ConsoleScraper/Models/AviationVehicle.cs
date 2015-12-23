using System;
using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IAviationVehicle
	{
		#region Properties
		string Name { get; set; }
		CountryEnum Country { get; set; }
		AviationVehicleTypeEnum VehicleType { get; set; }
		int Rank { get; set; }
		double BattleRating { get; set; }
		int MaxAltitude { get; set; }
		double MaxSpeed { get; set; }
		double TurnTime { get; set; }
		int TakeOffDistance { get; set; }
		string ClimbTime { get; set; }
		double ClimbRate { get; set; }
		TimeSpan TimeForFreeRepair { get; set; }
		long MaxRepairCost { get; set; }
		long PurchaseCost { get; set; }
		VehicleCostUnitHelper PurchaseCostUnit { get; set; }
		VehicleCostUnitHelper MaxRepairCostUnit { get; set; }
		VehicleSpeedUnitHelper MaxSpeedUnit { get; set; }
		VehicleEnginePowerUnitHelper EnginePowerUnit { get; set; }
		#endregion
	}

	public class AviationVehicle : IVehicle, IAviationVehicle
	{
		public string Name { get; set; }
		public CountryEnum Country { get; set; }
		public AviationVehicleTypeEnum VehicleType { get; set; }
		public int Rank { get; set; }
		public double BattleRating { get; set; }
		public int MaxAltitude { get; set; }
		public double MaxSpeed { get; set; }
		public double TurnTime { get; set; }
		public int TakeOffDistance { get; set; }
		public string ClimbTime { get; set; }
		public double ClimbRate { get; set; }
		public TimeSpan TimeForFreeRepair { get; set; }
		public long MaxRepairCost { get; set; }
		public long PurchaseCost { get; set; }
		public VehicleCostUnitHelper PurchaseCostUnit { get; set; }
		public VehicleCostUnitHelper MaxRepairCostUnit { get; set; }
		public VehicleSpeedUnitHelper MaxSpeedUnit { get; set; }
		public VehicleEnginePowerUnitHelper EnginePowerUnit { get; set; }
	}
}