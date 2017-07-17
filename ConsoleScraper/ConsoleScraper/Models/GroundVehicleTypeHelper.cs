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

		#endregion Properties

		#region Methods

		GroundVehicleTypeHelper GetGroundVehicleTypeFromName(string groundVehicleTypeName);

		#endregion Methods
	}

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