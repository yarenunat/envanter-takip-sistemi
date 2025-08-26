using System;
using System.Data;
using System.Data.SqlClient;

namespace WpfApp1.Repositories
{
    public class DashboardRepository
    {
        private readonly string _connectionString;

        public DashboardRepository()
        {
            _connectionString = "Server=YAREN\\SQLEXPRESS;Database=InvAssignSys_Prod;Trusted_Connection=True;";
        }

        // Toplam Ürün Sayısını Çeker
        public int GetTotalProductCount()
        {
            return GetCount("SELECT COUNT(*) FROM product0 WHERE IsActive = 1");
        }

        // Toplam Personel Sayısını Çeker
        public int GetEmployeeCount()
        {
            return GetCount("SELECT COUNT(*) FROM employee0");
        }

        // Bekleyen (Depodaki) Ürünlerin Sayısını Çeker
        public int GetPendingProductsCount()
        {
            // StatusID = 5 (Beklemede)
            return GetCount("SELECT COUNT(*) FROM product0 WHERE StatusID = 5 AND IsActive = 1");
        }

        // Arızalı Ürünlerin Sayısını Çeker
        public int GetDefectiveProductCount()
        {
            // StatusID = 2 (Arızalı)
            return GetCount("SELECT COUNT(*) FROM product0 WHERE StatusID = 2 AND IsActive = 1");
        }

        // Mevcut Ürünlerin Sayısını Çeker (opsiyonel)
        public int GetAvailableProductsCount()
        {
            // StatusID = 1 (Mevcut)
            return GetCount("SELECT COUNT(*) FROM product0 WHERE StatusID = 1 AND IsActive = 1");
        }

        // Hurda Ürünlerin Sayısını Çeker (opsiyonel)
        public int GetScrapProductsCount()
        {
            // StatusID = 3 (Hurda)
            return GetCount("SELECT COUNT(*) FROM product0 WHERE StatusID = 3 AND IsActive = 1");
        }

        // Zimmetli Ürünlerin Sayısını Çeker (opsiyonel)
        public int GetAssignedProductsCount()
        {
            // StatusID = 4 (Zimmetli)
            return GetCount("SELECT COUNT(*) FROM product0 WHERE StatusID = 4 AND IsActive = 1");
        }

        // SQL'den tek bir sayı değeri çeken genel metot
        private int GetCount(string query)
        {
            int count = 0;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        count = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Veri çekerken hata oluştu: {ex.Message}", ex);
            }
            return count;
        }
    }
}