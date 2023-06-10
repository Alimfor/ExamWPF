using CsvHelper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exda.ReportFile
{
    public class Report : IFileExtensions
    {
        public void GenerateCsvReport(string beginDate, string endDate)
        {
            DataTable resultsTable = GetSurveyResults(beginDate, endDate);

            using (var writer = new StreamWriter("report.csv", false, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField("Имя");
                csv.WriteField("Фамилия");
                csv.WriteField("Начало сдачи");
                csv.WriteField("Конец сдачи");
                csv.WriteField("результат_в_%");
                csv.NextRecord();

                foreach (DataRow row in resultsTable.Rows)
                {
                    csv.WriteField(row["FirstName"]);
                    csv.WriteField(row["LastName"]);
                    csv.WriteField(row["StartDate"]);
                    csv.WriteField(row["EndDate"]);
                    csv.WriteField(row["ResultInProcent"]);
                    csv.NextRecord();
                }
            }
        }

        public void GenerateExcelReport(string beginDate, string endDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            DataTable resultsTable = GetSurveyResults(beginDate, endDate);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                worksheet.Cells["A1"].Value = "Имя";
                worksheet.Cells["B1"].Value = "Фамилия";
                worksheet.Cells["C1"].Value = "Начало сдачи";
                worksheet.Cells["D1"].Value = "Конец сдачи";
                worksheet.Cells["E1"].Value = "результат_в_%";

                int rowIndex = 2;
                foreach (DataRow row in resultsTable.Rows)
                {
                    worksheet.Cells[$"A{rowIndex}"].Value = row["FirstName"];
                    worksheet.Cells[$"B{rowIndex}"].Value = row["LastName"];
                    worksheet.Cells[$"C{rowIndex}"].Value = ((DateTime)row["StartDate"]).ToString("yyyy-mm-dd"); ;
                    worksheet.Cells[$"D{rowIndex}"].Value = ((DateTime)row["EndDate"]).ToString("yyyy-mm-dd"); ;
                    worksheet.Cells[$"E{rowIndex}"].Value = row["ResultInProcent"];
                    rowIndex++;
                }

                File.WriteAllBytes("report.xlsx", package.GetAsByteArray());
            }
        }

        private DataTable GetSurveyResults(string beginDate, string endDate)
        {
            DataTable resultsTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["conStr"].ConnectionString))
            {
                connection.Open();

                string query = "SELECT FirstName, LastName, StartDate, EndDate, ResultInProcent FROM UserInfo " +
                               "WHERE StartDate >= @BeginDate AND EndDate <= @EndDate";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BeginDate", beginDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        adapter.Fill(resultsTable);
                }
            }
            foreach (DataRow row in resultsTable.Rows)
            {
                Console.WriteLine("FirstName: " + row["FirstName"]);
                Console.WriteLine("LastName: " + row["LastName"]);
                Console.WriteLine("StartDate: " + row["StartDate"]);
                Console.WriteLine("EndDate: " + row["EndDate"]);
                Console.WriteLine("ResultInProcent: " + row["ResultInProcent"]);
                Console.WriteLine();
            }
            return resultsTable;
        }
    }
}
