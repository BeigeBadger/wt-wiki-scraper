using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicle
	{
		string Title { get; set; }
		CountryEnum Country { get; set; }
		int Rank { get; set; }
		double MaxSpeed { get; set; }
		double BattleRating { get; set; }
		long Cost { get; set; }
		VehicleCostUnit PurchaseCostUnit { get; set; }
		VehicleCostUnit MaxRepairCostUnit { get; set; }
		VehicleSpeedUnit MaxSpeedUnit { get; set; }
		VehicleEnginePowerUnit EnginePowerUnit { get; set; }
	}
}