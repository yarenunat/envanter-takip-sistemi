using System.Collections.ObjectModel;

namespace WpfApp1.model
{
    public class EmployeeModel
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string RegistrationNumber { get; set; }
        public string Email { get; set; }
        public string DepartmentName { get; set; }
        
        public ObservableCollection<AssignedItemModel> AssignedItems { get; set; }

        public EmployeeModel()
        {
            AssignedItems = new ObservableCollection<AssignedItemModel>();
        }
    }

}
