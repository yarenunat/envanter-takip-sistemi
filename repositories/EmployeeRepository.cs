using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using WpfApp1.model;

namespace WpfApp1.Repositories
{
    public class EmployeeRepository : RepositoryBase, IEmployeeRepository
    {
        public EmployeeRepository() : base("InvAssignSys_Prod") { }

        private const string BaseEmployeeSelect = @"
SELECT
    e.EmployeeID         AS EmployeeId,
    e.FullName           AS FullName,
    e.RegistrationNumber AS RegistrationNumber,
    e.Email              AS Email,
    d.DepartmentName     AS DepartmentName
FROM employee0 e
LEFT JOIN department0 d ON d.DepartmentID = e.DepartmentID";

        public IEnumerable<EmployeeModel> GetEmployees()
        {
            var list = new List<EmployeeModel>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(BaseEmployeeSelect + " WHERE e.IsDeleted = 0 ORDER BY e.FullName;", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(MapEmployee(r));
                }
            }
            return list;
        }

        public IEnumerable<EmployeeModel> GetEmployeesFiltered(string categoryName, string departmentName)
        {
            var list = new List<EmployeeModel>();
            var sql = BaseEmployeeSelect + " WHERE 1=1 AND e.IsDeleted = 0";

            if (!string.IsNullOrWhiteSpace(departmentName))
                sql += " AND d.DepartmentName = @dept";

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                sql += @"
 AND EXISTS (
     SELECT 1
     FROM assignment0 a
     JOIN product0 p  ON p.ProductID  = a.ProductID
     JOIN category0 c ON c.CategoryID = p.CategoryID
     WHERE a.EmployeeID = e.EmployeeID
       AND a.IsCanceled = 0
       AND c.CategoryName = @cat
)";
            }

            sql += " ORDER BY e.FullName;";

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (!string.IsNullOrWhiteSpace(departmentName))
                    cmd.Parameters.AddWithValue("@dept", departmentName);
                if (!string.IsNullOrWhiteSpace(categoryName))
                    cmd.Parameters.AddWithValue("@cat", categoryName);

                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(MapEmployee(r));
                }
            }

            return list;
        }

        public EmployeeModel GetEmployeeByRegistrationNumber(string registrationNumber)
        {
            var sql = BaseEmployeeSelect + " WHERE e.RegistrationNumber = @regNo AND e.IsDeleted = 0;";
            EmployeeModel employee = null;

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@regNo", registrationNumber);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                        employee = MapEmployee(r);
                }
            }
            return employee;
        }

        public IEnumerable<AssignedItemModel> GetAssignedItems(int employeeId)
        {
            var list = new List<AssignedItemModel>();
            var sql = @"
SELECT
    p.ProductID          AS ProductId,
    c.CategoryName       AS CategoryName,
    p.Name               AS Name,
    p.Barcode            AS Barcode,
    p.SerialNumber       AS SerialNumber,
    a.AssignedAt         AS AssignedAt
FROM assignment0 a
JOIN product0 p  ON p.ProductID  = a.ProductID
JOIN category0 c ON c.CategoryID = p.CategoryID
WHERE a.EmployeeID = @empId
  AND a.IsCanceled = 0
ORDER BY a.AssignedAt DESC, p.Name;";

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@empId", employeeId);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(MapAssigned(r));
                }
            }

            return list;
        }

        public IEnumerable<string> GetAllCategories()
        {
            var list = new List<string>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("SELECT CategoryName FROM category0 ORDER BY CategoryName;", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(r["CategoryName"].ToString());
                }
            }
            return list;
        }

        public IEnumerable<string> GetAllDepartments()
        {
            var list = new List<string>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("SELECT DepartmentName FROM department0 ORDER BY DepartmentName;", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(r["DepartmentName"].ToString());
                }
            }
            return list;
        }

        private static EmployeeModel MapEmployee(IDataRecord r)
        {
            return new EmployeeModel
            {
                EmployeeId = r["EmployeeId"] == DBNull.Value ? 0 : Convert.ToInt32(r["EmployeeId"]),
                FullName = r["FullName"] == DBNull.Value ? string.Empty : r["FullName"].ToString(),
                RegistrationNumber = r["RegistrationNumber"] == DBNull.Value ? string.Empty : r["RegistrationNumber"].ToString(),
                DepartmentName = r["DepartmentName"] == DBNull.Value ? null : r["DepartmentName"].ToString(),
                Email = r["Email"] == DBNull.Value ? null : r["Email"].ToString()
            };
        }

        private static AssignedItemModel MapAssigned(IDataRecord r)
        {
            return new AssignedItemModel
            {
                ProductId = r["ProductId"] == DBNull.Value ? 0 : Convert.ToInt32(r["ProductId"]),
                CategoryName = r["CategoryName"] == DBNull.Value ? null : r["CategoryName"].ToString(),
                Name = r["Name"] == DBNull.Value ? null : r["Name"].ToString(),
                Barcode = r["Barcode"] == DBNull.Value ? null : r["Barcode"].ToString(),
                SerialNumber = r["SerialNumber"] == DBNull.Value ? null : r["SerialNumber"].ToString(),
                AssignedAt = r["AssignedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["AssignedAt"])
            };
        }

        public async Task DeleteEmployeeAndUnassignItemsAsync(int employeeId)
        {
            using (var conn = GetConnection())
            {
                await conn.OpenAsync();

                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var cancelCmd = new SqlCommand(@"
UPDATE assignment0
SET IsCanceled = 1
WHERE EmployeeID = @id AND IsCanceled = 0;", conn, tran);
                        cancelCmd.Parameters.AddWithValue("@id", employeeId);
                        await cancelCmd.ExecuteNonQueryAsync();

                        var deleteCmd = new SqlCommand(@"
UPDATE employee0
SET IsDeleted = 1
WHERE EmployeeID = @id;", conn, tran);
                        deleteCmd.Parameters.AddWithValue("@id", employeeId);
                        await deleteCmd.ExecuteNonQueryAsync();

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
