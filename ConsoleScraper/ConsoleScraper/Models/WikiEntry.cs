using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IWikiEntry
	{
		#region Properties
		string Title { get; set; }
		string Url { get; set; }
		VehicleTypeEnum VehicleType { get; set; }
		GroundVehicle GroundVehicleInfo { get; set; }
		AviationVehicle AviationVehicleInfo { get; set; }
		#endregion
	}

	public class WikiEntry : IWikiEntry
	{
		public string Title { get; set; }
		public string Url { get; set; }
		public VehicleTypeEnum VehicleType { get; set; }
		public GroundVehicle GroundVehicleInfo { get; set; }
		public AviationVehicle AviationVehicleInfo { get; set; }
	}
}