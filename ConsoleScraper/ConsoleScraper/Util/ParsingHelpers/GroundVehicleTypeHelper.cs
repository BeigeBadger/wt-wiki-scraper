using ConsoleScraper.Enums;
using ConsoleScraper.Models.Interfaces;
using ConsoleScraper.Util.Interfaces;

namespace ConsoleScraper.Util.ParsingHelpers
{
	public class GroundVehicleTypeHelper : IVehicleStatisticalUnit, IGroundVehicleTypeHelper
	{
		public int Id { get; set; }
		public GroundVehicleTypeEnum VehicleType { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		private const string LightTankFullName = "Light tank";
		private const string LightTankAbbreviation = "LT";
		private const string MediumTankFullName = "Medium tank";
		private const string MediumTankAbbreviation = "MT";
		private const string HeavyTankFullName = "Heavy tank";
		private const string HeavyTankAbbreviation = "HT";
		private const string TankDestroyerFullName = "Tank destroyer";
		private const string TankDestroyerAbbreviation = "TD";
		private const string AntiAircraftVehicleFullName = "Self propelled anti-aircraft";
		private const string AntiAircraftVehicleAbbreviation = "SPAA";

		public GroundVehicleTypeHelper()
		{
		}

		private GroundVehicleTypeHelper(GroundVehicleTypeEnum vehicleTypeEnum, string name, string abbreviation)
		{
			Id = (int)vehicleTypeEnum;
			VehicleType = vehicleTypeEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public GroundVehicleTypeHelper GetGroundVehicleTypeFromName(string groundVehicleTypeName)
		{
			// TODO: Make switch-case
			if (groundVehicleTypeName.Equals(LightTankFullName))
			{
				return new GroundVehicleTypeHelper(GroundVehicleTypeEnum.LightTank, LightTankFullName, LightTankAbbreviation);
			}
			if (groundVehicleTypeName.Equals(MediumTankFullName))
			{
				return new GroundVehicleTypeHelper(GroundVehicleTypeEnum.MediumTank, MediumTankFullName, MediumTankAbbreviation);
			}
			if (groundVehicleTypeName.Equals(HeavyTankFullName))
			{
				return new GroundVehicleTypeHelper(GroundVehicleTypeEnum.HeavyTank, HeavyTankFullName, HeavyTankAbbreviation);
			}
			if (groundVehicleTypeName.Equals(TankDestroyerFullName))
			{
				return new GroundVehicleTypeHelper(GroundVehicleTypeEnum.TankDestroyer, TankDestroyerFullName, TankDestroyerAbbreviation);
			}
			if (groundVehicleTypeName.Equals(AntiAircraftVehicleFullName))
			{
				return new GroundVehicleTypeHelper(GroundVehicleTypeEnum.AntiAircraftVehicle, AntiAircraftVehicleFullName, AntiAircraftVehicleAbbreviation);
			}

			return new GroundVehicleTypeHelper();
		}
	}
}