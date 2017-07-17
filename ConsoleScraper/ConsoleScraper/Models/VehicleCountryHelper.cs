using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicleCountryHelper
	{
		#region Properties

		int Id { get; set; }
		CountryEnum CountryEnum { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }

		#endregion Properties

		#region Methods

		VehicleCountryHelper GetVehicleCountryFromAbbreviation(string countryAbbreviation);

		VehicleCountryHelper GetVehicleCountryFromName(string countryName);

		#endregion Methods
	}

	public class VehicleCountryHelper : IVehicleStatisticalUnit, IVehicleCountryHelper
	{
		public int Id { get; set; }
		public CountryEnum CountryEnum { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		private const string UsaAbbreviation = "USA";
		private const string GermanyAbbreviation = "Germany";
		private const string UssrAbbreviation = "USSR";
		private const string BritainAbbreviation = "Great Britain";
		private const string JapanAbbreviation = "Japan";
		private const string ItalyAbbreviation = "Italy";
		private const string FranceAbbreviation = "France";
		private const string AustraliaAbbreviation = "Australia";
		private const string UsaName = "United States of America";
		private const string GermanyName = GermanyAbbreviation;
		private const string UssrName = UssrAbbreviation;
		private const string BritainName = BritainAbbreviation;
		private const string JapanName = JapanAbbreviation;
		private const string ItalyName = ItalyAbbreviation;
		private const string FranceName = FranceAbbreviation;
		private const string AustraliaName = AustraliaAbbreviation;

		public VehicleCountryHelper()
		{
		}

		private VehicleCountryHelper(CountryEnum countryEnum, string name, string abbreviation)
		{
			Id = (int)countryEnum;
			CountryEnum = countryEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleCountryHelper GetVehicleCountryFromAbbreviation(string countryAbbreviation)
		{
			// TODO: Make switch-case
			if (countryAbbreviation.Equals(UsaAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.Usa, CountryEnum.Usa.ToString(), UsaAbbreviation);
			}
			if (countryAbbreviation.Equals(GermanyAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.Germany, CountryEnum.Germany.ToString(), GermanyAbbreviation);
			}
			if (countryAbbreviation.Equals(UssrAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.Ussr, CountryEnum.Ussr.ToString(), UssrAbbreviation);
			}
			if (countryAbbreviation.Equals(BritainAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.GreatBritain, CountryEnum.GreatBritain.ToString(), BritainAbbreviation);
			}
			if (countryAbbreviation.Equals(JapanAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.Japan, CountryEnum.Japan.ToString(), JapanAbbreviation);
			}
			if (countryAbbreviation.Equals(ItalyAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.Italy, CountryEnum.Italy.ToString(), ItalyAbbreviation);
			}
			if (countryAbbreviation.Equals(FranceAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.France, CountryEnum.France.ToString(), FranceAbbreviation);
			}
			if (countryAbbreviation.Equals(AustraliaAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.Australia, CountryEnum.Australia.ToString(), AustraliaAbbreviation);
			}

			return new VehicleCountryHelper();
		}

		public VehicleCountryHelper GetVehicleCountryFromName(string countryName)
		{
			// TODO: Make switch-case
			if (countryName.Equals(UsaName))
			{
				return new VehicleCountryHelper(CountryEnum.Usa, CountryEnum.Usa.ToString(), UsaAbbreviation);
			}
			if (countryName.Equals(GermanyName))
			{
				return new VehicleCountryHelper(CountryEnum.Germany, CountryEnum.Germany.ToString(), GermanyAbbreviation);
			}
			if (countryName.Equals(UssrName))
			{
				return new VehicleCountryHelper(CountryEnum.Ussr, CountryEnum.Ussr.ToString(), UssrAbbreviation);
			}
			if (countryName.Equals(BritainName))
			{
				return new VehicleCountryHelper(CountryEnum.GreatBritain, CountryEnum.GreatBritain.ToString(), BritainAbbreviation);
			}
			if (countryName.Equals(JapanName))
			{
				return new VehicleCountryHelper(CountryEnum.Japan, CountryEnum.Japan.ToString(), JapanAbbreviation);
			}
			if (countryName.Equals(ItalyName))
			{
				return new VehicleCountryHelper(CountryEnum.Italy, CountryEnum.Italy.ToString(), ItalyAbbreviation);
			}
			if (countryName.Equals(FranceName))
			{
				return new VehicleCountryHelper(CountryEnum.France, CountryEnum.France.ToString(), FranceAbbreviation);
			}
			if (countryName.Equals(AustraliaName))
			{
				return new VehicleCountryHelper(CountryEnum.Australia, CountryEnum.Australia.ToString(), AustraliaAbbreviation);
			}

			return new VehicleCountryHelper();
		}
	}
}