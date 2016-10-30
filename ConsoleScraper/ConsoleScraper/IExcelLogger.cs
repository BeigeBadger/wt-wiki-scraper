using ConsoleScraper.Models;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System;
using ConsoleScraper.Enums;

namespace ConsoleScraper
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

	public class ExcelLogger : IExcelLogger
	{
		public ExcelLogger()
		{
		}

		public void AddGroundVehicleRowToSpreadsheet(GroundVehicle groundVehicle, ExcelWorksheet worksheet)
		{
			if (groundVehicle != null)
			{
				// Get the row we are up to
				int indexPosition = worksheet.Dimension.End.Row + 1;

				// Add values
				worksheet.Cells[$"A{indexPosition}"].Value = groundVehicle.Name;
				worksheet.Cells[$"B{indexPosition}"].Value = groundVehicle.Country;
				worksheet.Cells[$"C{indexPosition}"].Value = Enum.GetName(typeof(VehicleTypeEnum), groundVehicle.VehicleType);
				worksheet.Cells[$"D{indexPosition}"].Value = groundVehicle.Rank;
				worksheet.Cells[$"E{indexPosition}"].Value = groundVehicle.BattleRating;
				worksheet.Cells[$"F{indexPosition}"].Value = groundVehicle.Weight;
				worksheet.Cells[$"G{indexPosition}"].Value = groundVehicle.WeightUnit.Name;
				worksheet.Cells[$"H{indexPosition}"].Value = groundVehicle.EnginePower;
				worksheet.Cells[$"I{indexPosition}"].Value = groundVehicle.EnginePowerUnit.Name;
				worksheet.Cells[$"J{indexPosition}"].Value = groundVehicle.MaxSpeed;
				worksheet.Cells[$"K{indexPosition}"].Value = groundVehicle.MaxSpeedUnit.Name;
				worksheet.Cells[$"L{indexPosition}"].Value = groundVehicle.HullArmourThickness;
				worksheet.Cells[$"M{indexPosition}"].Value = groundVehicle.SuperstructureArmourThickness;
				worksheet.Cells[$"N{indexPosition}"].Value = groundVehicle.TimeForFreeRepair;
				worksheet.Cells[$"O{indexPosition}"].Value = groundVehicle.MaxRepairCost;
				worksheet.Cells[$"P{indexPosition}"].Value = groundVehicle.MaxRepairCostUnit.Name;
				worksheet.Cells[$"Q{indexPosition}"].Value = groundVehicle.PurchaseCost;
				worksheet.Cells[$"R{indexPosition}"].Value = groundVehicle.PurchaseCostUnit.Name;
				worksheet.Cells[$"S{indexPosition}"].Value = groundVehicle.LastModified;
			}
		}

		public void CreateGroundVehicleSpreadsheetHeaders(ExcelWorksheet worksheet)
		{
			//Headers
			worksheet.Cells["A1"].Value = "Name";
			worksheet.Cells["B1"].Value = "Country";
			worksheet.Cells["C1"].Value = "Vehicle Type";
			worksheet.Cells["D1"].Value = "Rank";
			worksheet.Cells["E1"].Value = "Battle Rating";
			worksheet.Cells["F1"].Value = "Weight";
			worksheet.Cells["G1"].Value = "Weight Unit";
			worksheet.Cells["H1"].Value = "Engine Power";
			worksheet.Cells["I1"].Value = "Engine Power Unit";
			worksheet.Cells["J1"].Value = "Max Speed";
			worksheet.Cells["K1"].Value = "Max Speed Unit";
			worksheet.Cells["L1"].Value = "Hull Armour Thickness";
			worksheet.Cells["M1"].Value = "Superstructure Armour Thickness";
			worksheet.Cells["N1"].Value = "Time For Free Repair";
			worksheet.Cells["O1"].Value = "Max Repair Cost";
			worksheet.Cells["P1"].Value = "Max Repair Cost Unit";
			worksheet.Cells["Q1"].Value = "Purchase Cost";
			worksheet.Cells["R1"].Value = "Purchase Cost Unit";
			worksheet.Cells["S1"].Value = "Last Modified";

			worksheet.Cells["A1:S1"].Style.Font.Bold = true;
		}

		public void CreateExcelFile(Dictionary<string, GroundVehicle> vehicleDetails)
		{
			// Setup objects to handle creating the spreadsheet
			FileInfo excelFile = new FileInfo($"{ConfigurationManager.AppSettings["LocalWikiExcelPath"]}GroundVehicleData.xlsx");
			ExcelPackage excelPackage = new ExcelPackage(excelFile);
			ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault() == null
				? excelPackage.Workbook.Worksheets.Add("Data")
				: excelPackage.Workbook.Worksheets.Single(w => w.Name == "Data");

			// Clear out old data before populating the headers again
			worksheet.DeleteColumn(1, 30);
			CreateGroundVehicleSpreadsheetHeaders(worksheet);

			// Populate spreadsheet
			Dictionary<string, GroundVehicle> orderedGroundVehicles = vehicleDetails.OrderBy(x => x.Key).ToDictionary(d => d.Key, d => d.Value);

			foreach (GroundVehicle groundVehicle in orderedGroundVehicles.Values)
			{
				AddGroundVehicleRowToSpreadsheet(groundVehicle, worksheet);
			}

			// Make columns fit content then save the file
			worksheet.Cells["A1:S1"].AutoFitColumns();
			excelPackage.Save();
		}
	}
}