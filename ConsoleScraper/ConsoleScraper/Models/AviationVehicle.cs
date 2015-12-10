using System;
using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IAviationVehicle
	{
		#region Properties
		string Title { get; set; }
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
		long Cost { get; set; }
		VehicleCostUnit PurchaseCostUnit { get; set; }
		VehicleCostUnit MaxRepairCostUnit { get; set; }
		VehicleSpeedUnit MaxSpeedUnit { get; set; }
		VehicleEnginePowerUnit EnginePowerUnit { get; set; }
		#endregion
	}

	public class AviationVehicle : IVehicle, IAviationVehicle
	{
		public string Title { get; set; }
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
		public long Cost { get; set; }
		public VehicleCostUnit PurchaseCostUnit { get; set; }
		public VehicleCostUnit MaxRepairCostUnit { get; set; }
		public VehicleSpeedUnit MaxSpeedUnit { get; set; }
		public VehicleEnginePowerUnit EnginePowerUnit { get; set; }
	}
}