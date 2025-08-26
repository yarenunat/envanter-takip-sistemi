using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.model;

namespace WpfApp1.Repositories
{
    public interface IEmployeeRepository
    {
        IEnumerable<EmployeeModel> GetEmployees();
        IEnumerable<EmployeeModel> GetEmployeesFiltered(string categoryName, string departmentName);
        IEnumerable<AssignedItemModel> GetAssignedItems(int employeeId);
        EmployeeModel GetEmployeeByRegistrationNumber(string registrationNumber);

        IEnumerable<string> GetAllCategories();
        IEnumerable<string> GetAllDepartments();
    }

}
