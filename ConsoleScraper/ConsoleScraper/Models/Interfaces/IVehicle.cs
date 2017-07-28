using ConsoleScraper.Enums;
using ConsoleScraper.Util.ParsingHelpers;

namespace ConsoleScraper.Models.Interfaces
{
	public interface IVehicle
	{
		long Id { get; set; }
		string Name { get; set; }
		CountryEnum Country { get; set; }
		VehicleTypeEnum VehicleType { get; set; }
		int Rank { get; set; }
		double MaxSpeed { get; set; }
		double BattleRating { get; set; }
		long PurchaseCost { get; set; }
		long MaxRepairCost { get; set; }
		string LastModified { get; set; }
		VehicleCostUnitHelper PurchaseCostUnit { get; set; }
		VehicleCostUnitHelper MaxRepairCostUnit { get; set; }
		VehicleSpeedUnitHelper MaxSpeedUnit { get; set; }
	}

	public class Vehicle : IVehicle
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public CountryEnum Country { get; set; }
		public VehicleTypeEnum VehicleType { get; set; }
		public int Rank { get; set; }
		public double MaxSpeed { get; set; }
		public double BattleRating { get; set; }
		public long PurchaseCost { get; set; }
		public long MaxRepairCost { get; set; }
		public string LastModified { get; set; }
		public VehicleCostUnitHelper PurchaseCostUnit { get; set; }
		public VehicleCostUnitHelper MaxRepairCostUnit { get; set; }
		public VehicleSpeedUnitHelper MaxSpeedUnit { get; set; }
	}
}