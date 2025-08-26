using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WpfApp1.model;
using System.Data;
using System.Threading.Tasks;

namespace WpfApp1.Repositories
{
    public class ManageRepository : RepositoryBase, IManageRepository
    {
        public ManageRepository() : base("InvAssignSys_Prod") { }

        // Kişi Yönetimi Metotları
        public IEnumerable<EmployeeModel> GetEmployees()
        {
            var list = new List<EmployeeModel>();
            var sql = @"
                SELECT 
                    e.EmployeeID,
                    e.FullName,
                    e.Email,
                    e.RegistrationNumber,
                    d.DepartmentName
                FROM employee0 e
                LEFT JOIN department0 d ON d.DepartmentID = e.DepartmentID
                ORDER BY e.FullName;";

            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new EmployeeModel
                        {
                            EmployeeId = r.GetInt32(r.GetOrdinal("EmployeeID")),
                            FullName = r["FullName"] as string,
                            Email = r["Email"] as string,
                            RegistrationNumber = r["RegistrationNumber"] as string,
                            DepartmentName = r["DepartmentName"] as string
                        });
                    }
                }
            }
            return list;
        }

        public IEnumerable<string> GetAllDepartments()
        {
            var list = new List<string>();
            var sql = "SELECT DepartmentName FROM department0 ORDER BY DepartmentName;";
            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(r.GetString(r.GetOrdinal("DepartmentName")));
                    }
                }
            }
            return list;
        }

        public IEnumerable<string> GetAllRoles()
        {
            var list = new List<string>();
            var sql = "SELECT RoleName FROM role0 ORDER BY RoleName;";
            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(r.GetString(r.GetOrdinal("RoleName")));
                    }
                }
            }
            return list;
        }

        public EmployeeModel AddEmployee(string fullName, string email, string departmentName, string registrationNumber)
        {
            const string sql = @"
                DECLARE @deptId INT = (SELECT TOP 1 DepartmentID FROM department0 WHERE DepartmentName = @dept);
                INSERT INTO employee0 (FullName, Email, Username, DepartmentID, RegistrationNumber)
                VALUES (@name, @mail, CONCAT('user', NEWID()), @deptId, @regNumber);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int newId;
            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = (object)fullName ?? DBNull.Value;
                cmd.Parameters.Add("@mail", SqlDbType.NVarChar).Value = (object)email ?? DBNull.Value;
                cmd.Parameters.Add("@dept", SqlDbType.NVarChar).Value = departmentName;
                cmd.Parameters.Add("@regNumber", SqlDbType.NVarChar).Value = (object)registrationNumber ?? DBNull.Value;

                c.Open();
                newId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return new EmployeeModel
            {
                EmployeeId = newId,
                FullName = fullName,
                Email = email,
                DepartmentName = departmentName,
                RegistrationNumber = registrationNumber
            };
        }

        public void UpdateEmployee(EmployeeModel employee)
        {
            var sql = @"
                DECLARE @deptId INT = (SELECT TOP 1 DepartmentID FROM department0 WHERE DepartmentName = @dept);
                UPDATE employee0
                SET FullName = @name,
                    Email = @mail,
                    DepartmentID = @deptId,
                    RegistrationNumber = @regNumber
                WHERE EmployeeID = @id;";
            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = employee.EmployeeId;
                cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = employee.FullName;
                cmd.Parameters.Add("@mail", SqlDbType.NVarChar).Value = employee.Email;
                cmd.Parameters.Add("@dept", SqlDbType.NVarChar).Value = employee.DepartmentName;
                cmd.Parameters.Add("@regNumber", SqlDbType.NVarChar).Value = (object)employee.RegistrationNumber ?? DBNull.Value;

                c.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public async Task DeleteEmployee(int employeeId)
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var assignedItems = GetAssignedItemsByEmployeeId(employeeId);
                        foreach (var item in assignedItems)
                        {
                            UnassignProductFromEmployee(employeeId, item.ProductId);
                            UpdateProductStatus(item.ProductId, "Beklemede");
                        }

                        var deleteSql = "DELETE FROM employee0 WHERE EmployeeID = @employeeId";
                        using (var cmd = new SqlCommand(deleteSql, connection, transaction))
                        {
                            cmd.Parameters.Add("@employeeId", SqlDbType.Int).Value = employeeId;
                            await cmd.ExecuteNonQueryAsync();
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

        public IEnumerable<AssignedItemModel> GetAssignedItemsByEmployeeId(int employeeId)
        {
            var assignments = new List<AssignedItemModel>();
            var sql = @"
                SELECT
                    p.ProductID, p.Name AS ProductName, p.Barcode,
                    a.EmployeeID, e.FullName AS EmployeeName, a.AssignedAt
                FROM assignment0 a
                JOIN product0 p ON a.ProductID = p.ProductID
                JOIN employee0 e ON a.EmployeeID = e.EmployeeID
                WHERE a.EmployeeID = @EmployeeId AND a.IsCanceled = 0 AND a.UnassignedAt IS NULL";

            using (var connection = GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        assignments.Add(new AssignedItemModel
                        {
                            ProductId = reader.GetInt32(reader.GetOrdinal("ProductID")),
                            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                            Barcode = reader.GetString(reader.GetOrdinal("Barcode")),
                            EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                            EmployeeName = reader.GetString(reader.GetOrdinal("EmployeeName")),
                            AssignedAt = reader.GetDateTime(reader.GetOrdinal("AssignedAt"))
                        });
                    }
                }
            }
            return assignments;
        }

        public void UnassignProductFromEmployee(int employeeId, int productId)
        {
            var sql = @"
                UPDATE a
                SET a.UnassignedAt = SYSDATETIME(), a.IsCanceled = 1
                FROM dbo.assignment0 a
                WHERE a.EmployeeID = @eid AND a.ProductID = @pid AND (a.IsCanceled = 0 OR a.IsCanceled IS NULL) AND a.UnassignedAt IS NULL;";

            using (var c = GetConnection())
            using (var tr = c.BeginTransaction())
            using (var cmd = new SqlCommand(sql, c, tr))
            {
                try
                {
                    cmd.Parameters.Add("@eid", SqlDbType.Int).Value = employeeId;
                    cmd.Parameters.Add("@pid", SqlDbType.Int).Value = productId;
                    cmd.ExecuteNonQuery();
                    tr.Commit();
                }
                catch
                {
                    tr.Rollback();
                    throw;
                }
            }
        }

        // Ürün Yönetimi Metotları
        public IEnumerable<ProductModel> GetProducts()
        {
            var list = new List<ProductModel>();
            var sql = @"
                SELECT 
                    p.ProductID,
                    p.Name,
                    c.CategoryName,
                    p.Brand,
                    p.Barcode,
                    p.PurchaseDate,
                    s.StatusName AS Status
                FROM product0 p
                LEFT JOIN category0 c ON c.CategoryID = p.CategoryID
                LEFT JOIN product_status0 s ON s.StatusID = p.StatusID
                ORDER BY p.Name;";

            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new ProductModel
                        {
                            Id = r.GetInt32(r.GetOrdinal("ProductID")),
                            Name = r["Name"] as string,
                            CategoryName = r["CategoryName"] as string,
                            Brand = r["Brand"] as string,
                            Barcode = r["Barcode"] as string,
                            PurchaseDate = r["PurchaseDate"] as DateTime?,
                            Status = r["Status"] as string
                        });
                    }
                }
            }
            return list;
        }

        public IEnumerable<string> GetAllCategories()
        {
            var list = new List<string>();
            var sql = "SELECT CategoryName FROM category0 ORDER BY CategoryName;";
            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(r.GetString(r.GetOrdinal("CategoryName")));
                    }
                }
            }
            return list;
        }

        public void AddProduct(ProductModel product)
        {
            var sql = @"
                DECLARE @catId INT = (SELECT TOP 1 CategoryID FROM category0 WHERE CategoryName = @cat);
                DECLARE @statusId INT = (SELECT TOP 1 StatusID FROM product_status0 WHERE StatusName = @status);
                INSERT INTO product0 (Name, CategoryID, Brand, Barcode, PurchaseDate, StatusID)
                VALUES (@name, @catId, @brand, @barcode, @pdate, @statusId);
                SELECT SCOPE_IDENTITY();";

            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = product.Name;
                cmd.Parameters.Add("@cat", SqlDbType.NVarChar).Value = product.CategoryName;
                cmd.Parameters.Add("@brand", SqlDbType.NVarChar).Value = (object)product.Brand ?? DBNull.Value;
                cmd.Parameters.Add("@barcode", SqlDbType.NVarChar).Value = product.Barcode;
                cmd.Parameters.Add("@pdate", SqlDbType.Date).Value = (object)product.PurchaseDate ?? DBNull.Value;
                cmd.Parameters.Add("@status", SqlDbType.NVarChar).Value = product.Status;

                c.Open();
                product.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void UpdateProduct(ProductModel product)
        {
            var sql = @"
                DECLARE @catId INT = (SELECT TOP 1 CategoryID FROM category0 WHERE CategoryName = @cat);
                DECLARE @statusId INT = (SELECT TOP 1 StatusID FROM product_status0 WHERE StatusName = @status);
                UPDATE product0
                SET Name = @name,
                    CategoryID = @catId,
                    Brand = @brand,
                    PurchaseDate = @pdate,
                    StatusID = @statusId
                WHERE ProductID = @id;";

            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = product.Id;
                cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = product.Name;
                cmd.Parameters.Add("@cat", SqlDbType.NVarChar).Value = product.CategoryName;
                cmd.Parameters.Add("@brand", SqlDbType.NVarChar).Value = (object)product.Brand ?? DBNull.Value;
                cmd.Parameters.Add("@pdate", SqlDbType.Date).Value = (object)product.PurchaseDate ?? DBNull.Value;
                cmd.Parameters.Add("@status", SqlDbType.NVarChar).Value = product.Status;

                c.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteProduct(int productId)
        {
            // Önce zimmet kayıtlarını silmek için sorgu
            var sql = @"
        DELETE FROM assignment0 WHERE ProductID = @pid;
        DELETE FROM product0 WHERE ProductID = @pid;";

            using (var c = GetConnection())
            {
                c.Open(); // <-- Bağlantı şimdi açılıyor
                using (var tr = c.BeginTransaction())
                using (var cmd = new SqlCommand(sql, c, tr))
                {
                    try
                    {
                        cmd.Parameters.Add("@pid", SqlDbType.Int).Value = productId;
                        cmd.ExecuteNonQuery();
                        tr.Commit();
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UpdateProductStatus(int productId, string status)
        {
            var sql = @"
                DECLARE @statusId INT = (SELECT TOP 1 StatusID FROM product_status0 WHERE StatusName = @status);
                UPDATE product0 SET StatusID = @statusId WHERE ProductID = @pid;";

            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                c.Open();
                cmd.Parameters.Add("@pid", SqlDbType.Int).Value = productId;
                cmd.Parameters.Add("@status", SqlDbType.NVarChar).Value = status;
                cmd.ExecuteNonQuery();
            }
        }

        public ProductModel GetProductByBarcode(string barcode)
        {
            ProductModel result = null;
            var sql = @"
                SELECT p.ProductID, p.Name, c.CategoryName, p.Brand, p.Barcode, p.PurchaseDate, ps.StatusName
                FROM product0 p
                JOIN category0 c ON p.CategoryID = c.CategoryID
                JOIN product_status0 ps ON p.StatusID = ps.StatusID
                WHERE Barcode = @Barcode";

            using (var connection = GetConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.Parameters.Add("@Barcode", SqlDbType.NVarChar).Value = barcode;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new ProductModel
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("ProductID")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                            Brand = reader["Brand"] as string,
                            Barcode = reader.GetString(reader.GetOrdinal("Barcode")),
                            PurchaseDate = reader["PurchaseDate"] as DateTime?,
                            Status = reader.GetString(reader.GetOrdinal("StatusName"))
                        };
                    }
                }
            }
            return result;
        }

        // Zimmet Yönetimi Metotları
        public IEnumerable<AssignedItemModel> GetAssignments()
        {
            var list = new List<AssignedItemModel>();
            var sql = @"
                SELECT 
                    a.AssignmentID, a.ProductID, p.Name AS ProductName, p.Barcode, a.EmployeeID,
                    e.FullName AS EmployeeName, a.AssignedAt, a.UnassignedAt, a.IsCanceled,
                    CASE WHEN a.IsCanceled = 1 THEN 'Pasif' ELSE 'Aktif' END AS Status
                FROM assignment0 a
                JOIN product0 p ON p.ProductID = a.ProductID
                JOIN employee0 e ON e.EmployeeID = a.EmployeeID
                ORDER BY a.AssignedAt DESC;";

            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new AssignedItemModel
                        {
                            Id = r.GetInt32(r.GetOrdinal("AssignmentID")),
                            ProductId = r.GetInt32(r.GetOrdinal("ProductID")),
                            ProductName = r["ProductName"] as string,
                            Barcode = r["Barcode"] as string,
                            EmployeeId = r.GetInt32(r.GetOrdinal("EmployeeID")),
                            EmployeeName = r["EmployeeName"] as string,
                            AssignedAt = r.GetDateTime(r.GetOrdinal("AssignedAt")),
                            UnassignedAt = r["UnassignedAt"] as DateTime?,
                            IsCanceled = r.GetBoolean(r.GetOrdinal("IsCanceled")),
                            Status = r["Status"] as string
                        });
                    }
                }
            }
            return list;
        }

        public AssignedItemModel GetActiveAssignmentByProductId(int productId)
        {
            AssignedItemModel result = null;
            var sql = @"
                SELECT TOP 1 
                    a.AssignmentID AS Id, a.ProductID, p.Name AS ProductName, a.EmployeeID,
                    e.FullName AS EmployeeName, a.AssignedAt, a.UnassignedAt, a.IsCanceled
                FROM assignment0 a
                JOIN product0 p ON p.ProductID = a.ProductID
                JOIN employee0 e ON a.EmployeeID = e.EmployeeID
                WHERE a.ProductID = @pid AND a.IsCanceled = 0 AND a.UnassignedAt IS NULL
                ORDER BY a.AssignedAt DESC;";

            using (var c = GetConnection())
            using (var cmd = new SqlCommand(sql, c))
            {
                cmd.Parameters.Add("@pid", SqlDbType.Int).Value = productId;
                c.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        result = new AssignedItemModel
                        {
                            Id = r.GetInt32(r.GetOrdinal("Id")),
                            ProductId = r.GetInt32(r.GetOrdinal("ProductID")),
                            ProductName = r["ProductName"] as string,
                            EmployeeId = r.GetInt32(r.GetOrdinal("EmployeeID")),
                            EmployeeName = r["EmployeeName"] as string,
                            AssignedAt = r.GetDateTime(r.GetOrdinal("AssignedAt")),
                            UnassignedAt = r["UnassignedAt"] as DateTime?,
                            IsCanceled = r.GetBoolean(r.GetOrdinal("IsCanceled"))
                        };
                    }
                }
            }
            return result;
        }

        public void AssignProduct(int employeeId, int productId)
        {
            var sqlUpdate = @"
                UPDATE assignment0 SET UnassignedAt = SYSDATETIME(), IsCanceled = 1
                WHERE ProductID = @pid AND IsCanceled = 0;";
            var sqlInsert = @"
                INSERT INTO assignment0 (ProductID, EmployeeID, AssignedAt, IsCanceled)
                VALUES (@pid, @eid, GETDATE(), 0);";
            var sqlStatus = @"
                UPDATE p
                SET p.StatusID = s.StatusID
                FROM product0 p
                JOIN product_status0 s ON s.StatusName = N'Zimmetli'
                WHERE p.ProductID = @pid;";

            using (var c = GetConnection())
            {
                c.Open();
                using (var tr = c.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        using (var cmd = new SqlCommand(sqlUpdate, c, tr))
                        {
                            cmd.Parameters.Add("@pid", SqlDbType.Int).Value = productId;
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand(sqlInsert, c, tr))
                        {
                            cmd.Parameters.Add("@pid", SqlDbType.Int).Value = productId;
                            cmd.Parameters.Add("@eid", SqlDbType.Int).Value = employeeId;
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand(sqlStatus, c, tr))
                        {
                            cmd.Parameters.Add("@pid", SqlDbType.Int).Value = productId;
                            cmd.ExecuteNonQuery();
                        }

                        tr.Commit();
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UnassignProduct(int productId)
        {
            var sqlUpdate = @"
                UPDATE a
                SET a.UnassignedAt = SYSDATETIME(), a.IsCanceled = 1
                FROM dbo.assignment0 a
                WHERE a.ProductID = @pid AND (a.IsCanceled = 0 OR a.IsCanceled IS NULL) AND a.UnassignedAt IS NULL;";

            var sqlStatus = @"
                UPDATE dbo.product0
                SET StatusID = (SELECT TOP 1 StatusID FROM product_status0 WHERE StatusName = 'Beklemede')
                WHERE ProductID = @pid;";

            using (var c = GetConnection())
            using (var tr = c.BeginTransaction())
            {
                try
                {
                    using (var cmd = new SqlCommand(sqlUpdate, c, tr))
                    {
                        cmd.Parameters.Add("@pid", SqlDbType.Int).Value = productId;
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand(sqlStatus, c, tr))
                    {
                        cmd.Parameters.Add("@pid", SqlDbType.Int).Value = productId;
                        cmd.ExecuteNonQuery();
                    }

                    tr.Commit();
                }
                catch
                {
                    tr.Rollback();
                    throw;
                }
            }
        }

        public void TransferAssignment(int newEmployeeId, int productId)
        {
            var sqlUpdate = @"
                UPDATE assignment0
                SET UnassignedAt = SYSDATETIME(), IsCanceled = 1
                WHERE ProductID = @pid AND IsCanceled = 0;";

            var sqlInsert = @"
                INSERT INTO assignment0 (ProductID, EmployeeID, AssignedAt, IsCanceled)
                VALUES (@pid, @eid, GETDATE(), 0);";

            using (var c = GetConnection())
            {
                c.Open();
                using (var tr = c.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        using (var cmd = new SqlCommand(sqlUpdate, c, tr))
                        {
                            cmd.Parameters.Add("@pid", SqlDbType.Int).Value = productId;
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand(sqlInsert, c, tr))
                        {
                            cmd.Parameters.Add("@pid", SqlDbType.Int).Value = productId;
                            cmd.Parameters.Add("@eid", SqlDbType.Int).Value = newEmployeeId;
                            cmd.ExecuteNonQuery();
                        }

                        tr.Commit();
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}