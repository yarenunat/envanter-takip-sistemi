using System;
using System.Data.SqlClient;

namespace WpfApp1.Helpers
{
    public static class AuditLogger
    {
        private static readonly string connectionString =
            "Server=YAREN\\SQLEXPRESS;Database=InvAssignSys_Prod;Trusted_Connection=True;";

        public static void Log(string action, string username)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    const string query = @"
                        INSERT INTO AuditLog (Action, Username)
                        VALUES (@action, @username)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@action", action);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Log yazılamadı: " + ex.Message);
            }
        }
    }
}
