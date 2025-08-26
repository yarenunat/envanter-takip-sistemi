using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.model;

namespace WpfApp1.Repositories
{
    public interface IEmployeeRepository
    {
        IEnumerable<EmployeeModel> GetEmployees();
        // Artık sadece categoryName ve departmentName parametreleri alıyor.
        IEnumerable<EmployeeModel> GetEmployeesFiltered(string categoryName, string departmentName);
        IEnumerable<AssignedItemModel> GetAssignedItems(int employeeId);

        // Arayüze GetEmployeeByRegistrationNumber metodu eklendi.
        EmployeeModel GetEmployeeByRegistrationNumber(string registrationNumber);

        IEnumerable<string> GetAllCategories();
        IEnumerable<string> GetAllDepartments();
        // GetAllRoles metodu arayüzden kaldırıldı.
    }
}