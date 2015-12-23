using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicle
	{
		string Name { get; set; }
		CountryEnum Country { get; set; }
		int Rank { get; set; }
		double MaxSpeed { get; set; }
		double BattleRating { get; set; }
		long PurchaseCost { get; set; }
		VehicleCostUnitHelper PurchaseCostUnit { get; set; }
		VehicleCostUnitHelper MaxRepairCostUnit { get; set; }
		VehicleSpeedUnitHelper MaxSpeedUnit { get; set; }
		VehicleEnginePowerUnitHelper EnginePowerUnit { get; set; }
	}
}