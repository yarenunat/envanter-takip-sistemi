using System.Configuration;
using System.Data.SqlClient;

namespace WpfApp1.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly string _connectionString;

        protected RepositoryBase(string connectionName)
        {
            _connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }
        public class ProductRepository : RepositoryBase
        {
            public ProductRepository() : base("InvAssignSys_Prod") { }
        }
        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
