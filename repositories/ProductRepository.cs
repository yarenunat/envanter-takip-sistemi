using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using WpfApp1.model;

namespace WpfApp1.Repositories
{
    public class ProductRepository : RepositoryBase, IProductRepository
    {
        public ProductRepository() : base("InvAssignSys_Prod") { }

        private const string SelectFromView = @"
SELECT 
    ProductID           AS Id,
    Barcode,
    Name,
    NULL                AS Description,
    NULL                AS Price,
    0                   AS Stock,
    PurchaseDate        AS CreatedDate,
    CategoryName,
    AssignedTo,
    AssignedDepartment,
    AssignedAt
FROM v_product0_list p";

        public IEnumerable<ProductModel> GetAll()
        {
            var list = new List<ProductModel>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand($"{SelectFromView} ORDER BY p.Name;", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(Map(r));
                }
            }
            return list;
        }

        public ProductModel GetById(int id)
        {
            ProductModel item = null;
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand($"{SelectFromView} WHERE p.Id = @id;", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                        item = Map(r);
                }
            }
            return item;
        }

        public ProductModel GetByBarcode(string search) => GetByBarcodeExact(search);
        public ProductModel GetProductByBarcode(string barcode) => GetByBarcodeExact(barcode);

        public async Task<IEnumerable<ProductModel>> GetAllAsync()
        {
            var list = new List<ProductModel>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand($"{SelectFromView} ORDER BY p.Name;", conn))
            {
                await conn.OpenAsync();
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(Map(r));
                }
            }
            return list;
        }

        public Task<ProductModel> GetByBarcodeAsync(string search) => GetByBarcodeExactAsync(search);

        public void Add(ProductModel product) => throw new NotImplementedException();
        public void Update(ProductModel product) => throw new NotImplementedException();
        public void Delete(int id) => throw new NotImplementedException();

        public Task<ProductModel> AddAsync(ProductModel product) =>
            Task.FromException<ProductModel>(new NotImplementedException());
        public Task<bool> RemoveAsync(int id) =>
            Task.FromException<bool>(new NotImplementedException());

        public ProductModel GetByBarcodeExact(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return null;

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand($"{SelectFromView} WHERE p.Barcode = @bc;", conn))
            {
                cmd.Parameters.AddWithValue("@bc", barcode.Trim());
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    return r.Read() ? Map(r) : null;
                }
            }
        }

        public async Task<ProductModel> GetByBarcodeExactAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return null;

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand($"{SelectFromView} WHERE p.Barcode = @bc;", conn))
            {
                cmd.Parameters.AddWithValue("@bc", barcode.Trim());
                await conn.OpenAsync();
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    return await r.ReadAsync() ? Map(r) : null;
                }
            }
        }
        public void InsertPendingOperation(int productId, string operationType)
        {
            var sql = @"
        INSERT INTO PendingOperations (ProductID, OperationType, CreatedAt, IsCompleted)
        VALUES (@productId, @operationType, @createdAt, 0);";

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@productId", productId);
                cmd.Parameters.AddWithValue("@operationType", operationType);
                cmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ————— ProductView için kısmi arama —————
        public IEnumerable<ProductModel> SearchByNameOrBarcode(string query)
        {
            var list = new List<ProductModel>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand($"{SelectFromView} WHERE p.Name LIKE @q OR p.Barcode LIKE @q ORDER BY p.Name", conn))
            {
                cmd.Parameters.AddWithValue("@q", $"%{query}%");
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(Map(r));
                }
            }
            return list;
        }

        public async Task<IEnumerable<ProductModel>> SearchByNameOrBarcodeAsync(string query)
        {
            var list = new List<ProductModel>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand($"{SelectFromView} WHERE p.Name LIKE @q OR p.Barcode LIKE @q ORDER BY p.Name", conn))
            {
                cmd.Parameters.AddWithValue("@q", $"%{query}%");
                await conn.OpenAsync();
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(Map(r));
                }
            }
            return list;
        }

        // ————— ScanView için stored procedure —————
        private ProductModel ScanLookup(string q)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("sp_scan0_lookup", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@q", q ?? string.Empty);

                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;

                    return new ProductModel
                    {
                        Id = SafeInt(r, "ProductID"),
                        Barcode = SafeString(r, "Barcode"),
                        Name = SafeString(r, "Name"),
                        Description = null,
                        Price = null,
                        Stock = 0,
                        CreatedDate = SafeDate(r, "PurchaseDate") ?? DateTime.MinValue,
                        CategoryName = SafeStringOrNull(r, "CategoryName"),
                        AssignedTo = SafeStringOrNull(r, "AssignedTo"),
                        AssignedDepartment = SafeStringOrNull(r, "AssignedDepartment"),
                        AssignedAt = SafeDate(r, "AssignedAt")
                    };
                }
            }
        }

        private async Task<ProductModel> ScanLookupAsync(string q)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("sp_scan0_lookup", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@q", q ?? string.Empty);

                await conn.OpenAsync();
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    if (!await r.ReadAsync()) return null;

                    return new ProductModel
                    {
                        Id = SafeInt(r, "ProductID"),
                        Barcode = SafeString(r, "Barcode"),
                        Name = SafeString(r, "Name"),
                        Description = null,
                        Price = null,
                        Stock = 0,
                        CreatedDate = SafeDate(r, "PurchaseDate") ?? DateTime.MinValue,
                        CategoryName = SafeStringOrNull(r, "CategoryName"),
                        AssignedTo = SafeStringOrNull(r, "AssignedTo"),
                        AssignedDepartment = SafeStringOrNull(r, "AssignedDepartment"),
                        AssignedAt = SafeDate(r, "AssignedAt")
                    };
                }
            }
        }

        private static ProductModel Map(IDataRecord r)
        {
            return new ProductModel
            {
                Id = SafeInt(r, "Id"),
                Barcode = SafeString(r, "Barcode"),
                Name = SafeString(r, "Name"),
                Description = SafeStringOrNull(r, "Description"),
                Price = SafeDecimalOrNull(r, "Price"),
                Stock = SafeIntOrDefault(r, "Stock"),
                CreatedDate = SafeDate(r, "CreatedDate") ?? DateTime.MinValue,
                CategoryName = SafeStringOrNull(r, "CategoryName"),
                AssignedTo = SafeStringOrNull(r, "AssignedTo"),
                AssignedDepartment = SafeStringOrNull(r, "AssignedDepartment"),
                AssignedAt = SafeDate(r, "AssignedAt")
            };
        }

        public IEnumerable<string> GetAllCategories()
        {
            var list = new List<string>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand("SELECT DISTINCT CategoryName FROM v_product0_list ORDER BY CategoryName", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(r.GetString(0));
                }
            }
            return list;
        }

        public IEnumerable<ProductModel> GetByCategory(string categoryName)
        {
            var list = new List<ProductModel>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand($"{SelectFromView} WHERE CategoryName = @cat ORDER BY p.Name", conn))
            {
                cmd.Parameters.AddWithValue("@cat", categoryName);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(Map(r));
                }
            }
            return list;
        }


        private static string SafeString(IDataRecord r, string name) =>
            r[name] == DBNull.Value ? string.Empty : r[name].ToString();

        private static string SafeStringOrNull(IDataRecord r, string name) =>
            r[name] == DBNull.Value ? null : r[name].ToString();

        private static int SafeInt(IDataRecord r, string name) =>
            r[name] == DBNull.Value ? 0 : Convert.ToInt32(r[name]);

        private static int SafeIntOrDefault(IDataRecord r, string name, int def = 0) =>
            r[name] == DBNull.Value ? def : Convert.ToInt32(r[name]);

        private static decimal? SafeDecimalOrNull(IDataRecord r, string name) =>
            r[name] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r[name]);

        private static DateTime? SafeDate(IDataRecord r, string name) =>
            r[name] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r[name]);
    }
}
