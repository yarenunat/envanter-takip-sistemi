using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using WpfApp1.model;

namespace WpfApp1.Repositories
{
    // IDepotRepository arayüzünü uyguladığından emin ol
    public class DepotRepository : IDepotRepository
    {
        private readonly string _connectionString;

        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public DepotRepository()
        {
            _connectionString = "Server=yaren\\SQLEXPRESS;Database=InvAssignSys_Prod;Trusted_Connection=True;";
        }

        public IEnumerable<DepotItem> GetDepotItems(string category = null, string status = null)
        {
            var items = new List<DepotItem>();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();

                    var query = @"
            SELECT
                p.ProductID,
                p.Name AS ProductName,
                p.Barcode,
                c.CategoryName,
                pl.LocationName,
                p.PurchaseDate,
                ps.StatusName
            FROM product0 p
            LEFT JOIN category0 c ON p.CategoryID = c.CategoryID
            LEFT JOIN product_location0 pl ON p.LocationID = pl.LocationID
            LEFT JOIN product_status0 ps ON p.StatusID = ps.StatusID
            WHERE ps.StatusID IN (1, 2, 3, 5) -- Mevcut(1), Arızalı(2), Hurda(3), Beklemede(5)
            AND (@category IS NULL OR c.CategoryName = @category)
            AND (@status IS NULL OR ps.StatusName = @status)
            ORDER BY p.ProductID DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@category",
                            string.IsNullOrEmpty(category) ? (object)DBNull.Value : category);
                        cmd.Parameters.AddWithValue("@status",
                            string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new DepotItem
                                {
                                    ProductID = reader.GetInt32(0),
                                    ProductName = reader.GetString(1),
                                    Barcode = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Category = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    LocationName = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    PurchaseDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                    StatusName = reader.IsDBNull(6) ? null : reader.GetString(6)
                                };
                                items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Veritabanı hatası: {ex.Message}");
                throw;
            }
            return items;
        }
        public IEnumerable<string> GetAllCategories()
        {
            var categories = new List<string>();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    var query = @"
            SELECT DISTINCT c.CategoryName 
            FROM category0 c
            JOIN product0 p ON c.CategoryID = p.CategoryID
            JOIN product_status0 ps ON p.StatusID = ps.StatusID
            WHERE ps.StatusName IN ('Mevcut', 'Arızalı', 'Hurda', 'Beklemede')
            ORDER BY c.CategoryName";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Kategori yükleme hatası: {ex.Message}");
                throw;
            }
            return categories;
        }

        public IEnumerable<string> GetStatusNames()
        {
            var statuses = new List<string>();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    // "Zimmetli" hariç tüm statüleri seç
                    var query = "SELECT StatusName FROM product_status0 WHERE StatusName != 'Zimmetli' ORDER BY StatusName";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            statuses.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Statü yükleme hatası: {ex.Message}");
                throw;
            }
            return statuses;
        }

        public IEnumerable<string> GetLocations()
        {
            var locations = new List<string>();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    var query = "SELECT DISTINCT LocationName FROM product_location0 ORDER BY LocationName";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            locations.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Konum yükleme hatası: {ex.Message}");
                throw;
            }
            return locations;
        }

        public IEnumerable<EmployeeModel> GetEmployees()
        {
            var employees = new List<EmployeeModel>();

            // SQL sorgusunu, employee0 tablosundaki DepartmentID'ye göre department0 tablosundan DepartmentName'i alacak şekilde güncelledim
            var sql = @"
        SELECT 
            e.EmployeeId, 
            e.FullName, 
            d.DepartmentName 
        FROM employee0 e
        LEFT JOIN department0 d ON e.DepartmentID = d.DepartmentID
        ORDER BY e.FullName";

            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                try
                {
                    c.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            employees.Add(new EmployeeModel
                            {
                                EmployeeId = r.GetInt32(0),
                                FullName = r.GetString(1),
                                DepartmentName = r.IsDBNull(2) ? null : r.GetString(2)
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Hata detayını Debug penceresine yazdır
                    Debug.WriteLine($"SQL Hatası: {ex.Message}");
                    // Uygulamanın çökmesini engellemek için, hatayı yeniden fırlatabilir veya loglayabilirsiniz.
                    throw;
                }
            }
            return employees;
        }

        public void UpdateProductStatus(int productId, string statusName)
        {
            var sql = "UPDATE product0 SET StatusID = (SELECT StatusID FROM product_status0 WHERE StatusName = @StatusName) WHERE ProductID = @ProductID";
            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.AddWithValue("@StatusName", statusName);
                cmd.Parameters.AddWithValue("@ProductID", productId);
                c.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void AssignProduct(int productId, int employeeId)
        {
            var updateProductSql = "UPDATE product0 SET StatusID = (SELECT StatusID FROM product_status0 WHERE StatusName = 'Zimmetli') WHERE ProductID = @ProductID";
            var insertAssignmentSql = "INSERT INTO assignment0 (ProductID, EmployeeID, AssignedAt) VALUES (@ProductID, @EmployeeID, GETDATE())";

            using (var conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(updateProductSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", productId);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand(insertAssignmentSql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", productId);
                            cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}