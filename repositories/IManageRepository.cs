using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.model;

namespace WpfApp1.Repositories
{
    public interface IManageRepository
    {
        // Kişi
        IEnumerable<EmployeeModel> GetEmployees();
        IEnumerable<string> GetAllDepartments();
        IEnumerable<string> GetAllRoles();
        EmployeeModel AddEmployee(string fullName, string email, string departmentName, string registrationNumber); // ✨ Düzeltilen metot tanımı
        void UpdateEmployee(EmployeeModel employee);
        Task DeleteEmployee(int employeeId);
        IEnumerable<AssignedItemModel> GetAssignedItemsByEmployeeId(int employeeId);

        // Zimmet
        void UnassignProductFromEmployee(int employeeId, int productId);
        void UpdateProductStatus(int productId, string status);

        // Ürün
        IEnumerable<ProductModel> GetProducts();
        IEnumerable<string> GetAllCategories();
        void AddProduct(ProductModel product);
        void UpdateProduct(ProductModel product);
        void DeleteProduct(int productId);
        ProductModel GetProductByBarcode(string barcode);

        // Zimmet
        IEnumerable<AssignedItemModel> GetAssignments();
        AssignedItemModel GetActiveAssignmentByProductId(int productId);
        void AssignProduct(int employeeId, int productId);
        void UnassignProduct(int productId);
        void TransferAssignment(int newEmployeeId, int productId);
    }
}