using ConsoleScraper.Enums;

namespace ConsoleScraper.Models
{
	public interface IVehicleCountry
	{
		#region Properties
		int Id { get; set; }
		CountryEnum CountryEnum { get; set; }
		string Name { get; set; }
		string Abbreviation { get; set; }
		#endregion

		#region Methods
		VehicleCountry GetVehicleCountryFromAbbreviation(string countryAbbreviation);
		VehicleCountry GetVehicleCountryFromName(string countryName);
		#endregion
	}

	public class VehicleCountry : IVehicleCountry
	{
		public int Id { get; set; }
		public CountryEnum CountryEnum { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }

		public const string UsaAbbreviation = "USA";
		public const string GermanyAbbreviation = "Germany";
		public const string UssrAbbreviation = "USSR";
		public const string BritainAbbreviation = "Britain";
		public const string JapanAbbreviation = "Japan";
		public const string UsaName = "United States of America";
		public const string GermanyName = GermanyAbbreviation;
		public const string UssrName = UssrAbbreviation;
		public const string BritainName = "Great Britain";
		public const string JapanName = JapanAbbreviation;

		public VehicleCountry() { }

		public VehicleCountry(CountryEnum countryEnum, string name, string abbreviation)
		{
			CountryEnum = countryEnum;
			Name = name;
			Abbreviation = abbreviation;
		}

		public VehicleCountry GetVehicleCountryFromAbbreviation(string countryAbbreviation)
		{
			if (countryAbbreviation.Equals(UsaAbbreviation))
			{
				return new VehicleCountry(CountryEnum.USA, CountryEnum.USA.ToString(), UsaAbbreviation);
			}
			else if (countryAbbreviation.Equals(GermanyAbbreviation))
			{
				return new VehicleCountry(CountryEnum.Germany, CountryEnum.Germany.ToString(), GermanyAbbreviation);
				
			}
			else if (countryAbbreviation.Equals(UssrAbbreviation))
			{
				return new VehicleCountry(CountryEnum.USSR, CountryEnum.USSR.ToString(), UssrAbbreviation);
				
			}
			else if (countryAbbreviation.Equals(BritainAbbreviation))
			{
				return new VehicleCountry(CountryEnum.Britain, CountryEnum.Britain.ToString(), BritainAbbreviation);
				
			}
			else if (countryAbbreviation.Equals(JapanAbbreviation))
			{
				return new VehicleCountry(CountryEnum.Japan, CountryEnum.Japan.ToString(), JapanAbbreviation);
				
			}

			return new VehicleCountry();
		}

		public VehicleCountry GetVehicleCountryFromName(string countryName)
		{
			if (countryName.Equals(UsaName))
			{
				return new VehicleCountry(CountryEnum.USA, CountryEnum.USA.ToString(), UsaAbbreviation);
			}
			else if (countryName.Equals(GermanyName))
			{
				return new VehicleCountry(CountryEnum.Germany, CountryEnum.Germany.ToString(), GermanyAbbreviation);

			}
			else if (countryName.Equals(UssrAbbreviation))
			{
				return new VehicleCountry(CountryEnum.USSR, CountryEnum.USSR.ToString(), UssrAbbreviation);

			}
			else if (countryName.Equals(BritainAbbreviation))
			{
				return new VehicleCountry(CountryEnum.Britain, CountryEnum.Britain.ToString(), BritainAbbreviation);

			}
			else if (countryName.Equals(JapanAbbreviation))
			{
				return new VehicleCountry(CountryEnum.Japan, CountryEnum.Japan.ToString(), JapanAbbreviation);

			}

			return new VehicleCountry();
		}
	}
}