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

		#endregion

		#region Methods

		VehicleCountryHelper GetVehicleCountryFromAbbreviation(string countryAbbreviation);
		VehicleCountryHelper GetVehicleCountryFromName(string countryName);

		#endregion
	}

	public class VehicleCountryHelper : IVehicleStatisticalUnit, IVehicleCountryHelper
	{
		public int Id { get; set; }
		public CountryEnum CountryEnum { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public const string UsaAbbreviation = "USA";
		public const string GermanyAbbreviation = "Germany";
		public const string UssrAbbreviation = "USSR";
		public const string BritainAbbreviation = "Great Britain";
		public const string JapanAbbreviation = "Japan";
		public const string ItalyAbbreviation = "Italy";
		public const string FranceAbbreviation = "France";
		public const string AustraliaAbbreviation = "Australia";
		public const string UsaName = "United States of America";
		public const string GermanyName = GermanyAbbreviation;
		public const string UssrName = UssrAbbreviation;
		public const string BritainName = BritainAbbreviation;
		public const string JapanName = JapanAbbreviation;
		public const string ItalyName = ItalyAbbreviation;
		public const string FranceName = FranceAbbreviation;
		public const string AustraliaName = AustraliaAbbreviation;

		public VehicleCountryHelper() { }

		public VehicleCountryHelper(CountryEnum countryEnum, string name, string abbreviation)
		{
			Id = (int)countryEnum;
			CountryEnum = countryEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleCountryHelper GetVehicleCountryFromAbbreviation(string countryAbbreviation)
		{
			if (countryAbbreviation.Equals(UsaAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.USA, CountryEnum.USA.ToString(), UsaAbbreviation);
			}
			else if (countryAbbreviation.Equals(GermanyAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.Germany, CountryEnum.Germany.ToString(), GermanyAbbreviation);				
			}
			else if (countryAbbreviation.Equals(UssrAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.USSR, CountryEnum.USSR.ToString(), UssrAbbreviation);
			}
			else if (countryAbbreviation.Equals(BritainAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.GreatBritain, CountryEnum.GreatBritain.ToString(), BritainAbbreviation);
			}
			else if (countryAbbreviation.Equals(JapanAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.Japan, CountryEnum.Japan.ToString(), JapanAbbreviation);
			}
			else if (countryAbbreviation.Equals(ItalyAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.Italy, CountryEnum.Italy.ToString(), ItalyAbbreviation);
			}
			else if (countryAbbreviation.Equals(FranceAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.France, CountryEnum.France.ToString(), FranceAbbreviation);
			}
			else if (countryAbbreviation.Equals(AustraliaAbbreviation))
			{
				return new VehicleCountryHelper(CountryEnum.Australia, CountryEnum.Australia.ToString(), AustraliaAbbreviation);
			}

			return new VehicleCountryHelper();
		}

		public VehicleCountryHelper GetVehicleCountryFromName(string countryName)
		{
			if (countryName.Equals(UsaName))
			{
				return new VehicleCountryHelper(CountryEnum.USA, CountryEnum.USA.ToString(), UsaAbbreviation);
			}
			else if (countryName.Equals(GermanyName))
			{
				return new VehicleCountryHelper(CountryEnum.Germany, CountryEnum.Germany.ToString(), GermanyAbbreviation);
			}
			else if (countryName.Equals(UssrName))
			{
				return new VehicleCountryHelper(CountryEnum.USSR, CountryEnum.USSR.ToString(), UssrAbbreviation);
			}
			else if (countryName.Equals(BritainName))
			{
				return new VehicleCountryHelper(CountryEnum.GreatBritain, CountryEnum.GreatBritain.ToString(), BritainAbbreviation);
			}
			else if (countryName.Equals(JapanName))
			{
				return new VehicleCountryHelper(CountryEnum.Japan, CountryEnum.Japan.ToString(), JapanAbbreviation);
			}
			else if (countryName.Equals(ItalyName))
			{
				return new VehicleCountryHelper(CountryEnum.Italy, CountryEnum.Italy.ToString(), ItalyAbbreviation);
			}
			else if (countryName.Equals(FranceName))
			{
				return new VehicleCountryHelper(CountryEnum.France, CountryEnum.France.ToString(), FranceAbbreviation);
			}
			else if (countryName.Equals(AustraliaName))
			{
				return new VehicleCountryHelper(CountryEnum.Australia, CountryEnum.Australia.ToString(), AustraliaAbbreviation);
			}

			return new VehicleCountryHelper();
		}
	}
}