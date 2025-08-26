using System;
using System.Collections.ObjectModel;
using System.Linq;
using WpfApp1.model;
using WpfApp1.Repositories;

namespace WpfApp1.viewModel
{
    public class EmployeeViewModel : ViewModelBase
    {
        private readonly IEmployeeRepository _repo;

        public ObservableCollection<EmployeeModel> Employees { get; set; }
        public ObservableCollection<string> CategoryList { get; set; }
        public ObservableCollection<string> DepartmentList { get; set; }
        // RoleList özelliği kaldırıldı

        private EmployeeModel _selectedEmployee;
        public EmployeeModel SelectedEmployee
        {
            get { return _selectedEmployee; }
            set
            {
                if (_selectedEmployee != value)
                {
                    _selectedEmployee = value;
                    OnPropertyChanged(nameof(SelectedEmployee));
                    LoadAssignedItems(_selectedEmployee);
                }
            }
        }

        private string _selectedCategory;
        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory));
                    ApplyFilters();
                }
            }
        }

        private string _selectedDepartment;
        public string SelectedDepartment
        {
            get { return _selectedDepartment; }
            set
            {
                if (_selectedDepartment != value)
                {
                    _selectedDepartment = value;
                    OnPropertyChanged(nameof(SelectedDepartment));
                    ApplyFilters();
                }
            }
        }

        // SelectedRole özelliği kaldırıldı

        // Arama metni: 'd' yazınca d ile başlayanlar listelenecek
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    ApplyFilters();
                }
            }
        }

        public EmployeeViewModel()
        {
            _repo = new EmployeeRepository();
            Employees = new ObservableCollection<EmployeeModel>();
            CategoryList = new ObservableCollection<string>();
            DepartmentList = new ObservableCollection<string>();

            LoadFilters();
            LoadEmployees();
        }

        public void LoadFilters()
        {
            CategoryList.Clear();
            DepartmentList.Clear();
            // RoleList'in temizlenmesi kaldırıldı

            foreach (var c in _repo.GetAllCategories()) CategoryList.Add(c);
            foreach (var d in _repo.GetAllDepartments()) DepartmentList.Add(d);
            // GetAllRoles çağrısı kaldırıldı

            OnPropertyChanged(nameof(CategoryList));
            OnPropertyChanged(nameof(DepartmentList));
            // RoleList'in PropertyChanged çağrısı kaldırıldı
        }

        public void LoadEmployees()
        {
            Employees.Clear();
            var list = _repo.GetEmployees();
            foreach (var e in list)
                Employees.Add(e);

            SelectedEmployee = Employees.FirstOrDefault();
            OnPropertyChanged(nameof(Employees));
        }

        // Seçili filtreler + arama metni ile listeyi yeniler
        public void ApplyFilters()
        {
            // GetEmployeesFiltered metodu artık 3 değil, 2 parametre alıyor.
            var list = _repo.GetEmployeesFiltered(SelectedCategory, SelectedDepartment);

            // Arama: 'd' → FullName veya EmployeeId 'd' ile başlıyorsa
            var q = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
            if (!string.IsNullOrEmpty(q))
            {
                list = list.Where(e =>
                    (!string.IsNullOrEmpty(e.FullName) &&
                        e.FullName.StartsWith(q, StringComparison.OrdinalIgnoreCase))
                    ||
                    // Sicil numarası ile arama eklendi
                    (!string.IsNullOrEmpty(e.RegistrationNumber) &&
                        e.RegistrationNumber.StartsWith(q, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            Employees.Clear();
            foreach (var e in list)
                Employees.Add(e);

            SelectedEmployee = Employees.FirstOrDefault();
            OnPropertyChanged(nameof(Employees));
        }

        public void LoadAssignedItems(EmployeeModel employee)
        {
            if (employee == null) return;

            employee.AssignedItems.Clear();
            var items = _repo.GetAssignedItems(employee.EmployeeId);
            foreach (var it in items)
                employee.AssignedItems.Add(it);
        }

        public void ClearFilters()
        {
            _selectedCategory = null;
            _selectedDepartment = null;
            _searchText = null;

            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(SelectedDepartment));
            OnPropertyChanged(nameof(SearchText));

            LoadEmployees();
        }

        // Kullanıcı adından çalışan seçimi (dokunulmadı)
        public void SelectEmployeeByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username) || Employees == null)
                return;

            var target = username.Trim();

            // Sicil numarasına göre arama da eklendi
            var match = Employees.FirstOrDefault(e =>
                string.Equals((e.FullName ?? string.Empty).Trim(),
                              target,
                              StringComparison.OrdinalIgnoreCase) ||
                string.Equals((e.RegistrationNumber ?? string.Empty).Trim(),
                              target,
                              StringComparison.OrdinalIgnoreCase));

            if (match != null)
                SelectedEmployee = match;
        }
    }
}