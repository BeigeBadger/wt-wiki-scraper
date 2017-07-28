using ConsoleScraper.Enums;
using ConsoleScraper.Util.ParsingHelpers;

namespace ConsoleScraper.Util.Interfaces
{
	public interface IGroundVehicleTypeHelper
	{
		#region Properties

		int Id { get; set; }
		GroundVehicleTypeEnum VehicleType { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }

		#endregion Properties

		#region Methods

		GroundVehicleTypeHelper GetGroundVehicleTypeFromName(string groundVehicleTypeName);

		#endregion Methods
	}
}