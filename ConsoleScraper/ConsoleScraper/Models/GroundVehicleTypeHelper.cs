using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IGroundVehicleTypeHelper
	{
		#region Properties
		int Id { get; set; }
		GroundVehicleTypeEnum VehicleType { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }
		#endregion

		#region Methods
		GroundVehicleTypeHelper GetGroundVehicleTypeFromName(string groundVehicleTypeName);
		#endregion
	}

	public class GroundVehicleTypeHelper : IVehicleStatisticalUnit, IGroundVehicleTypeHelper
	{
		public int Id { get; set; }
		public GroundVehicleTypeEnum VehicleType { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public const string LightTankFullName = "Light tank";
		public const string LightTankAbbreviation = "LT";
		public const string MediumTankFullName = "Medium tank";
		public const string MediumTankAbbreviation = "MT";
		public const string HeavyTankFullName = "Heavy tank";
		public const string HeavyTankAbbreviation = "HT";
		public const string TankDestroyerFullName = "Tank destroyer";
		public const string TankDestroyerAbbreviation = "TD";
		public const string AntiAircraftVehicleFullName = "Self propelled anti-aircraft";
		public const string AntiAircraftVehicleAbbreviation = "SPAA";

		public GroundVehicleTypeHelper() { }

		public GroundVehicleTypeHelper(GroundVehicleTypeEnum vehicleTypeEnum, string name, string abbreviation)
		{
			Id = (int)vehicleTypeEnum;
			VehicleType = vehicleTypeEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public GroundVehicleTypeHelper GetGroundVehicleTypeFromName(string groundVehicleTypeName)
		{
			if (groundVehicleTypeName.Equals(LightTankFullName))
			{
				return new GroundVehicleTypeHelper(GroundVehicleTypeEnum.LightTank, LightTankFullName, LightTankAbbreviation);
			}
			else if (groundVehicleTypeName.Equals(MediumTankFullName))
			{
				return new GroundVehicleTypeHelper(GroundVehicleTypeEnum.MediumTank, MediumTankFullName, MediumTankAbbreviation);
			}
			else if (groundVehicleTypeName.Equals(HeavyTankFullName))
			{
				return new GroundVehicleTypeHelper(GroundVehicleTypeEnum.HeavyTank, HeavyTankFullName, HeavyTankAbbreviation);
			}
			else if (groundVehicleTypeName.Equals(TankDestroyerFullName))
			{
				return new GroundVehicleTypeHelper(GroundVehicleTypeEnum.TankDestroyer, TankDestroyerFullName, TankDestroyerAbbreviation);
			}
			else if (groundVehicleTypeName.Equals(AntiAircraftVehicleFullName))
			{
				return new GroundVehicleTypeHelper(GroundVehicleTypeEnum.AntiAircraftVehicle, AntiAircraftVehicleFullName, AntiAircraftVehicleAbbreviation);
			}

			return new GroundVehicleTypeHelper();
		}
	}
}