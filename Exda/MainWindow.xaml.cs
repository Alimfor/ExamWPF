using CsvHelper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Exda
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DispatcherTimer timer;
        private int elapsedSeconds;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            elapsedSeconds = 0;

            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e) => elapsedSeconds++;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsEmptyFields())
            {
                MessageBox.Show("Вы не ввели данные");
                return;
            }
            timer.Stop();

            WriteUserAnswer();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["conStr"].ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("CheckUserAnswers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable resultsTable = new DataTable();
                        adapter.Fill(resultsTable);

                        caLabel.Content = resultsTable.Compute("COUNT(Result)", "Result = 'Правильно'");
                        totalQuesLabel.Content = resultsTable.Rows.Count;
                        procentLabel.Content = int.Parse((string)caLabel.Content) / int.Parse((string)totalQuesLabel.Content);
                        timeLabel.Content = elapsedSeconds;
                    }
                }
            }
        }

        private void WriteUserAnswer()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["conStr"].ConnectionString))
            {
                connection.Open();

                int count = 1;
                while(count < 6)
                {
                    string query = "INSERT INTO UserAnswers (QuestionId, UserAnswer) VALUES (@QuestionId, @UserAnswer)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@QuestionId", count);
                        command.Parameters.AddWithValue("@UserAnswer", $"tb{count}.Text");

                        command.ExecuteNonQuery();
                    }
                    count++;
                }
            }
        }

        private void GenerateCsvReport(string beginDate, string endDate)
        {
            DataTable resultsTable = GetSurveyResults(beginDate, endDate);

            // Создание объекта StreamWriter для записи в файл CSV
            using (var writer = new StreamWriter("report.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Запись заголовков столбцов
                csv.WriteField("Дата_теста");
                csv.WriteField("ФИО");
                csv.WriteField("результат_в_%");
                csv.NextRecord();

                // Запись данных по результатам анкетирования
                foreach (DataRow row in resultsTable.Rows)
                {
                    csv.WriteField(row["Дата_теста"]);
                    csv.WriteField(row["ФИО"]);
                    csv.WriteField(row["результат_в_%"]);
                    csv.NextRecord();
                }
            }
        }

        private void GenerateExcelReport(string beginDate, string endDate)
        {
            // Получение результатов анкетирования из базы данных
            DataTable resultsTable = GetSurveyResults(beginDate, endDate);

            // Создание нового пакета Excel
            using (var package = new ExcelPackage())
            {
                // Создание нового листа в пакете
                var worksheet = package.Workbook.Worksheets.Add("Report");

                // Запись заголовков столбцов
                worksheet.Cells["A1"].Value = "Дата_теста";
                worksheet.Cells["B1"].Value = "ФИО";
                worksheet.Cells["C1"].Value = "результат_в_%";

                // Запись данных по результатам анкетирования
                int rowIndex = 2;
                foreach (DataRow row in resultsTable.Rows)
                {
                    worksheet.Cells[$"A{rowIndex}"].Value = row["Дата_теста"];
                    worksheet.Cells[$"B{rowIndex}"].Value = row["ФИО"];
                    worksheet.Cells[$"C{rowIndex}"].Value = row["результат_в_%"];
                    rowIndex++;
                }

                File.WriteAllBytes("report.xlsx", package.GetAsByteArray());
            }
        }

        private DataTable GetSurveyResults(string beginDate, string endDate)
        {
            DataTable resultsTable = new DataTable();

            using (SqlConnection connection = new SqlConnection("YourConnectionString"))
            {
                connection.Open();

                string query = "SELECT Дата_теста, ФИО, результат_в_% FROM YourTable " +
                               "WHERE Дата_теста >= @BeginDate AND Дата_теста <= @EndDate";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BeginDate", beginDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                // Выполнение команды SQL и заполнение таблицы результатами
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(resultsTable);
            }

            return resultsTable;
        }
        private bool IsEmptyFields()
        {
            return tbName.Text.Equals("") ||
                   tbLastName.Text.Equals("") ||
                   tb1.Text.Equals("") ||
                   tb2.Text.Equals("") ||
                   tb3.Text.Equals("") ||
                   tb4.Text.Equals("") ||
                   tb5.Text.Equals("");
        }
    }
}
