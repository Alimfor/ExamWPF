using CsvHelper;
using Exda.ReportFile;
using Exda.WorkingWithDB;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
        private DateRange dateRange;
        private WorkingWithUser working;

        private DispatcherTimer timer;
        private int elapsedSeconds;

        public MainWindow()
        {
            InitializeComponent();

            dateRange = new DateRange();
            elapsedSeconds = 0;
        }
        private void StartTimer()
        {
            if (elapsedSeconds == 0)
            {
                dateRange.startDate = DateTime.Now;
            }

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;

            timer.Start();
        }
        private void StopTimer()
        {
            if (timer == null) return;

            timer.Stop();
            dateRange.endDate = DateTime.Now;
        }
        private void Timer_Tick(object sender, EventArgs e) => elapsedSeconds++;

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            StopTimer();
            working = new WorkingWithUser(dateRange);

            working.WriteInfoAboutUser(tbLastName, tbName);
            working.WriteUserAnswer(testingGrid);
            working.Check();

            ShowUserStatistics(working.GetCheckedAnswer());

            gb.Visibility = Visibility.Visible;
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            if (menuItem.Header.ToString().Equals("Тестирование"))
            {
                testingGrid.Visibility = Visibility.Visible;
                StartTimer();
                reportGrid.Visibility = Visibility.Collapsed;
            }
            else if (menuItem.Header.ToString().Equals("Загрузить отчет"))
            {
                StopTimer();
                testingGrid.Visibility = Visibility.Collapsed;
                reportGrid.Visibility = Visibility.Visible;
            }
        }
        private void ReportBtn_Click(object sender, RoutedEventArgs e)
        {
            IFileExtensions file = new Report();

            if (fileTypeCb.SelectedItem != null)
            {
                ComboBoxItem item = fileTypeCb.SelectedItem as ComboBoxItem;
                string selectedFileType = item.Content.ToString();

                if (selectedFileType.Equals("csv"))  
                    file.GenerateCsvReport(startDateTb.Text, endDateTb.Text);
                else 
                    file.GenerateExcelReport(startDateTb.Text, endDateTb.Text);

                MessageBox.Show("Saccessful", "Отчет успешно загржен");
            }
        }

        private void ShowUserStatistics(DataTable resultsTable)
        {
            double correctAnswers = int.Parse(resultsTable.Compute("COUNT(Result)", "Result = 'Правильно'").ToString());
            int totalQuestions = resultsTable.Rows.Count;
            double procent = correctAnswers / totalQuestions * 100;

            caLabel.Content = correctAnswers;
            totalQuesLabel.Content = totalQuestions;
            procentLabel.Content = procent;
            timeLabel.Content = elapsedSeconds;

            working.UpdateResultInProcent(procent);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (IsNumericInput(e.Text))
            {
                e.Handled = true;
            }
        }
        private void TextBox_PreviewNumericInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsNumericInput(e.Text))
            {
                e.Handled = true;
            }
        }
        private bool IsNumericInput(string text) => int.TryParse(text, out _);


    }
}
