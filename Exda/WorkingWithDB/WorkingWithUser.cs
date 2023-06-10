using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Exda.WorkingWithDB
{
    public class WorkingWithUser : IUserInformation, ICheckingUserResponse
    {
        private readonly DateRange dateRange;
        private DataTable checkedTable;
        public WorkingWithUser(DateRange dateRange) => this.dateRange = dateRange;

        public void Check()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["conStr"].ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("CheckUserAnswers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserInfoId", GetLastInsertedUserInfoId());

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable resultsTable = new DataTable();
                        adapter.Fill(resultsTable);
                        checkedTable = resultsTable;
                        foreach (DataRow row in resultsTable.Rows)
                        {
                            foreach (DataColumn column in resultsTable.Columns)
                            {
                                Console.Write(row[column] + "\t");
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }
        }
        public void WriteInfoAboutUser(TextBox tbLastName,TextBox tbName)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["conStr"].ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("InsertUserInfo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LastName", tbLastName.Text);
                    command.Parameters.AddWithValue("@FirstName", tbName.Text);
                    command.Parameters.AddWithValue("@StartDate", dateRange.startDate);
                    command.Parameters.AddWithValue("@EndDate", dateRange.endDate);



                    object status = command.ExecuteScalar();

                    if (status.ToString().Equals("1"))
                    {
                        MessageBox.Show("Вы не ввели фамилию/имя");
                    }
                }
            }
        }
        public void WriteUserAnswer(Grid testingGrid)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["conStr"].ConnectionString))
            {
                connection.Open();

                int quesId = 1;
                foreach (var control in testingGrid.Children)
                {
                    if (control is TextBox textBox && !textBox.Name.Equals("tbName") && !textBox.Name.Equals("tbLastName"))
                    {
                        using (SqlCommand command = new SqlCommand("InsertUserAnswers", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@QuestionId", quesId);
                            command.Parameters.AddWithValue("@UserAnswer", textBox.Text);
                            command.Parameters.AddWithValue("@UserInfoId", GetLastInsertedUserInfoId());

                            object status = command.ExecuteScalar();

                            switch (status.ToString())
                            {
                                case "1":
                                    MessageBox.Show("Вы не ответили на вопрос");
                                    return;
                                case "2":
                                    MessageBox.Show("QuestionIdError", "Извините проблема  на стороне приложения");
                                    return;
                                case "3":
                                    MessageBox.Show("UserInfoIdError", "Извините проблема  на стороне приложения");
                                    return;
                                default:
                                    break;
                            }
                        }
                        quesId++;
                    }
                }
                MessageBox.Show("Saccessful", "Ответы успешно записаны!");
            }
        }
        public void UpdateResultInProcent(double resultInProcent)
        {
            try
            {
                if (resultInProcent < 0 || resultInProcent > 100)
                {
                    throw new ArgumentException("Значение должно быть целым числом от 0 до 100.", nameof(resultInProcent));
                }

                UpdateResultInProcent(GetLastInsertedUserInfoId(), resultInProcent);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
            
        }
        public DataTable GetCheckedAnswer() => checkedTable;

        private void UpdateResultInProcent(int userInfoId, double resultInProcent)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["conStr"].ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("UPDATE UserInfo SET ResultInProcent = @ResultInProcent WHERE UserInfoId = @UserInfoId", connection))
                {
                    command.Parameters.AddWithValue("@ResultInProcent", resultInProcent);
                    command.Parameters.AddWithValue("@UserInfoId", userInfoId);

                    command.ExecuteNonQuery();
                }
            }
        }
        private int GetLastInsertedUserInfoId()
        {
            int userInfoId = -1;

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["conStr"].ConnectionString))
            {
                SqlCommand command = new SqlCommand("SELECT IDENT_CURRENT('UserInfo')", connection);

                connection.Open();
                userInfoId = Convert.ToInt32(command.ExecuteScalar());
            }

            return userInfoId;
        }
    }
}
