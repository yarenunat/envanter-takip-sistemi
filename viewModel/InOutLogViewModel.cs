using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using WpfApp1.model;

namespace WpfApp1.viewModel
{
    public class InOutLogViewModel : ViewModelBase
    {
        private readonly string connectionString =
            "Server=yaren\\SQLEXPRESS;Database=InvAssignSys_Prod;Trusted_Connection=True;";

        public ObservableCollection<InOutLogItem> InOutItems { get; set; }

        public InOutLogViewModel()
        {
            InOutItems = new ObservableCollection<InOutLogItem>();
            LoadLogsFromDatabase();
        }

        public void LoadLogsFromDatabase()
        {
            try
            {
                InOutItems.Clear();

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    const string query = @"
                        SELECT Action, Timestamp
                        FROM AuditLog
                        ORDER BY Timestamp DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var action = reader["Action"]?.ToString() ?? "";
                            var timestamp = reader["Timestamp"] != DBNull.Value
                                ? reader.GetDateTime(reader.GetOrdinal("Timestamp"))
                                : DateTime.MinValue;

                            string opType = action;
                            string opDesc = "";

                            // ":" ile parçala
                            var parts = action.Split(new[] { ':' }, 2);
                            if (parts.Length > 1)
                            {
                                opType = parts[0].Trim();
                                opDesc = parts[1].Trim();
                            }

                            InOutItems.Add(new InOutLogItem
                            {
                                OperationType = opType,
                                ActionDescription = opDesc,
                                Time = timestamp
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loglar yüklenirken hata oluştu: " + ex.Message,
                                "Hata",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Log ekler. Eğer açıklama boşsa işlem tipine göre otomatik üretilir.
        /// </summary>
        public void AddLog(string actionType, string username, string personName = null, string customDescription = null)
        {
            string description = customDescription;

            if (string.IsNullOrWhiteSpace(description))
            {
                if (actionType == "Yeni Kişi Eklendi" && !string.IsNullOrWhiteSpace(personName))
                {
                    description = personName + " kişisi eklendi";
                }
                else if (actionType == "Kişi Silindi" && !string.IsNullOrWhiteSpace(personName))
                {
                    description = personName + " kişisi silindi";
                }
                else
                {
                    description = "";
                }
            }

            // DB'ye "Yapılan İşlem:Açıklama" formatında yaz
            string combinedAction = actionType + ":" + description;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    const string insertQuery = @"
                        INSERT INTO AuditLog (Action, Username)
                        VALUES (@action, @username)";

                    using (var cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@action", combinedAction);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.ExecuteNonQuery();
                    }
                }

                // UI'ya anında ekle
                InOutItems.Insert(0, new InOutLogItem
                {
                    OperationType = actionType,
                    ActionDescription = description,
                    Time = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Log eklenirken hata oluştu: " + ex.Message,
                                "Hata",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}
