using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient; // C# 7.3 uyumlu
using System.Threading.Tasks;
using WpfApp1.model;

namespace WpfApp1.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly string _connStr;

        public AssignmentRepository()
        {
            _connStr = ConfigurationManager.ConnectionStrings["InvAssignSys_Prod"].ConnectionString;
        }

        public async Task<IEnumerable<AssignedItemModel>> GetAllAsync()
        {
            const string sql = @"
SELECT 
    a.AssignmentID,
    a.ProductID,
    a.EmployeeID,
    a.AssignedAt,
    a.UnassignedAt,
    a.IsCanceled,
    p.Name              AS ProductName,
    p.Brand,
    p.Model,
    p.Barcode,
    p.SerialNumber,
    p.StatusID,
    s.StatusName,
    p.LocationID,
    l.LocationName,
    p.IsActive,
    p.Price,
    e.FullName          AS EmployeeName
FROM assignment0 a
INNER JOIN product0 p           ON p.ProductID = a.ProductID
LEFT  JOIN product_status0 s    ON s.StatusID = p.StatusID
LEFT  JOIN product_location0 l  ON l.LocationID = p.LocationID
LEFT  JOIN employee0 e          ON e.EmployeeID = a.EmployeeID;";

            var list = new List<AssignedItemModel>();
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;
                await conn.OpenAsync();
                using (var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    while (await rdr.ReadAsync())
                        list.Add(MapAssignedItem(rdr));
                }
            }
            return list;
        }

        public async Task<IEnumerable<AssignedItemModel>> GetPendingAssignmentsAsync()
        {
            // Sahipsiz (aktif zimmeti olmayan) ve durumu 'Mevcut' olan ürünler
            const string sql = @"
SELECT 
    L.AssignmentID,
    p.ProductID,
    L.EmployeeID,
    L.AssignedAt,
    L.UnassignedAt,
    L.IsCanceled,
    p.Name              AS ProductName,
    p.Brand,
    p.Model,
    p.Barcode,
    p.SerialNumber,
    p.StatusID,
    s.StatusName,
    p.LocationID,
    l.LocationName,
    p.IsActive,
    p.Price,
    e.FullName          AS EmployeeName
FROM product0 p
LEFT  JOIN product_status0 s    ON s.StatusID = p.StatusID
LEFT  JOIN product_location0 l  ON l.LocationID = p.LocationID
OUTER APPLY (
    SELECT TOP 1 a.*
    FROM assignment0 a
    WHERE a.ProductID = p.ProductID
    ORDER BY a.AssignedAt DESC, a.AssignmentID DESC
) L
LEFT  JOIN employee0 e          ON e.EmployeeID = L.EmployeeID
WHERE 
    p.IsActive = 1
    AND (s.StatusName = N'Mevcut' OR s.StatusName IS NULL)
    AND NOT EXISTS (
        SELECT 1
        FROM assignment0 ax
        WHERE ax.ProductID = p.ProductID
          AND ax.IsCanceled = 0
          AND ax.UnassignedAt IS NULL
    )
ORDER BY L.UnassignedAt DESC, p.Name;";

            var list = new List<AssignedItemModel>();
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;
                await conn.OpenAsync();
                using (var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    while (await rdr.ReadAsync())
                        list.Add(MapAssignedItem(rdr));
                }
            }
            return list;
        }

        public async Task<AssignedItemModel> GetLatestByProductIdAsync(int productId)
        {
            const string sql = @"
WITH L AS (
    SELECT TOP 1
        a.AssignmentID,
        a.ProductID,
        a.EmployeeID,
        a.AssignedAt,
        a.UnassignedAt,
        a.IsCanceled
    FROM assignment0 a
    WHERE a.ProductID = @ProductID
    ORDER BY a.AssignedAt DESC, a.AssignmentID DESC
)
SELECT 
    L.AssignmentID,
    L.ProductID,
    L.EmployeeID,
    L.AssignedAt,
    L.UnassignedAt,
    L.IsCanceled,
    p.Name              AS ProductName,
    p.Brand,
    p.Model,
    p.Barcode,
    p.SerialNumber,
    p.StatusID,
    s.StatusName,
    p.LocationID,
    l.LocationName,
    p.IsActive,
    p.Price,
    e.FullName          AS EmployeeName
FROM L
INNER JOIN product0 p           ON p.ProductID = L.ProductID
LEFT  JOIN product_status0 s    ON s.StatusID = p.StatusID
LEFT  JOIN product_location0 l  ON l.LocationID = p.LocationID
LEFT  JOIN employee0 e          ON e.EmployeeID = L.EmployeeID;";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = productId });
                await conn.OpenAsync();
                using (var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                {
                    if (await rdr.ReadAsync())
                        return MapAssignedItem(rdr);
                }
            }
            return null;
        }

        public async Task<AssignedItemModel> AssignAsync(int employeeId, int productId)
        {
            // Önce aktif zimmeti kapat, sonra yeni zimmeti oluştur ve ürünü 'Zimmetli' yap
            const string sql = @"
UPDATE assignment0
SET UnassignedAt = SYSDATETIME(), IsCanceled = 1
WHERE ProductID = @ProductID AND IsCanceled = 0 AND UnassignedAt IS NULL;

INSERT INTO assignment0 (ProductID, EmployeeID, AssignedAt, IsCanceled, UnassignedAt)
VALUES (@ProductID, @EmployeeID, SYSDATETIME(), 0, NULL);

DECLARE @NewId INT = SCOPE_IDENTITY();

UPDATE p
SET p.StatusID = s.StatusID
FROM product0 p
INNER JOIN product_status0 s ON s.StatusName = N'Zimmetli'
WHERE p.ProductID = @ProductID;

SELECT @NewId;";

            int newId;
            using (var conn = new SqlConnection(_connStr))
            {
                await conn.OpenAsync();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(sql, conn, tran))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = productId });
                            cmd.Parameters.Add(new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId });
                            var scalar = await cmd.ExecuteScalarAsync();
                            newId = Convert.ToInt32(scalar);
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }

            return await GetLatestByProductIdAsync(productId);
        }

        public async Task<bool> UnassignAsync(int assignmentId)
        {
            // Zimmeti kapat (IsCanceled=1) ve ürünü 'Mevcut' yap
            const string sql = @"
DECLARE @ProductID INT, @Affected INT;

SELECT @ProductID = ProductID 
FROM assignment0 
WHERE AssignmentID = @AssignmentID;

UPDATE assignment0
SET UnassignedAt = SYSDATETIME(),
    IsCanceled   = 1
WHERE AssignmentID = @AssignmentID
  AND (UnassignedAt IS NULL OR IsCanceled = 0);

SET @Affected = @@ROWCOUNT;

UPDATE p
SET p.StatusID = s.StatusID
FROM product0 p
INNER JOIN product_status0 s ON s.StatusName = N'Mevcut'
WHERE p.ProductID = @ProductID;

SELECT @Affected;";

            int affected = 0;
            using (var conn = new SqlConnection(_connStr))
            {
                await conn.OpenAsync();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(sql, conn, tran))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add(new SqlParameter("@AssignmentID", SqlDbType.Int) { Value = assignmentId });
                            var scalar = await cmd.ExecuteScalarAsync();
                            affected = Convert.ToInt32(scalar ?? 0);
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
            return affected > 0;
        }

        private static AssignedItemModel MapAssignedItem(SqlDataReader rdr)
        {
            int oAssignmentID = rdr.GetOrdinal("AssignmentID");
            int oProductID = rdr.GetOrdinal("ProductID");
            int oEmployeeID = rdr.GetOrdinal("EmployeeID");
            int oAssignedAt = rdr.GetOrdinal("AssignedAt");
            int oUnassignedAt = rdr.GetOrdinal("UnassignedAt");
            int oIsCanceled = rdr.GetOrdinal("IsCanceled");
            int oProductName = rdr.GetOrdinal("ProductName");
            int oBrand = rdr.GetOrdinal("Brand");
            int oModel = rdr.GetOrdinal("Model");
            int oBarcode = rdr.GetOrdinal("Barcode");
            int oSerialNumber = rdr.GetOrdinal("SerialNumber");
            int oStatusID = rdr.GetOrdinal("StatusID");
            int oStatusName = rdr.GetOrdinal("StatusName");
            int oLocationID = rdr.GetOrdinal("LocationID");
            int oLocationName = rdr.GetOrdinal("LocationName");
            int oIsActive = rdr.GetOrdinal("IsActive");
            int oPrice = rdr.GetOrdinal("Price");
            int oEmployeeName = rdr.GetOrdinal("EmployeeName");

            var m = new AssignedItemModel
            {
                AssignmentId = rdr.IsDBNull(oAssignmentID) ? 0 : rdr.GetInt32(oAssignmentID),
                ProductId = rdr.IsDBNull(oProductID) ? 0 : rdr.GetInt32(oProductID),
                EmployeeId = rdr.IsDBNull(oEmployeeID) ? 0 : rdr.GetInt32(oEmployeeID),
                AssignedAt = rdr.IsDBNull(oAssignedAt) ? (DateTime?)null : rdr.GetDateTime(oAssignedAt),
                UnassignedAt = rdr.IsDBNull(oUnassignedAt) ? (DateTime?)null : rdr.GetDateTime(oUnassignedAt),
                IsCanceled = !rdr.IsDBNull(oIsCanceled) && rdr.GetBoolean(oIsCanceled),
                ProductName = rdr.IsDBNull(oProductName) ? null : rdr.GetString(oProductName),
                Brand = rdr.IsDBNull(oBrand) ? null : rdr.GetString(oBrand),
                Model = rdr.IsDBNull(oModel) ? null : rdr.GetString(oModel),
                Barcode = rdr.IsDBNull(oBarcode) ? null : rdr.GetString(oBarcode),
                SerialNumber = rdr.IsDBNull(oSerialNumber) ? null : rdr.GetString(oSerialNumber),
                StatusId = rdr.IsDBNull(oStatusID) ? 0 : rdr.GetInt32(oStatusID),
                StatusName = rdr.IsDBNull(oStatusName) ? null : rdr.GetString(oStatusName),
                LocationId = rdr.IsDBNull(oLocationID) ? 0 : rdr.GetInt32(oLocationID),
                LocationName = rdr.IsDBNull(oLocationName) ? null : rdr.GetString(oLocationName),
                IsActive = !rdr.IsDBNull(oIsActive) && rdr.GetBoolean(oIsActive),
                Price = rdr.IsDBNull(oPrice) ? (decimal?)null : rdr.GetDecimal(oPrice),
                EmployeeName = rdr.IsDBNull(oEmployeeName) ? null : rdr.GetString(oEmployeeName)
            };
            return m;
        }
    }
}
