using ConsoleScraper.Models;
using OfficeOpenXml;
using System.Collections.Generic;

namespace ConsoleScraper.Logging.Interfaces
{
	public interface IExcelLogger
	{
		/// <summary>
		/// Adds an entry for a ground vehicle to a spreadsheet
		/// </summary>
		/// <param name="groundVehicle">The vehicle to pull the information from</param>
		/// <param name="worksheet">The worksheet to add the data to</param>
		void AddGroundVehicleRowToSpreadsheet(GroundVehicle groundVehicle, ExcelWorksheet worksheet);

		/// <summary>
		/// Creates the headers for the Ground Vehicles spreadsheet
		/// </summary>
		/// <param name="worksheet">The worksheet to create the headers in</param>
		void CreateGroundVehicleSpreadsheetHeaders(ExcelWorksheet worksheet);

		/// <summary>
		/// Creates a new excel file that holds all of the details extracted from
		/// the wiki in the location specified by the App.Config file
		/// </summary>
		void CreateExcelFile(Dictionary<string, GroundVehicle> vehicleDetails);
	}
}