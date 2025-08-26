using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Repositories;

namespace WpfApp1.viewModel
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly DashboardRepository _dashboardRepository;
        private readonly MainViewModel _mainViewModel;

        // Özellikler (Properties)
        private string _username;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private int _totalProductCount;
        public int TotalProductCount
        {
            get => _totalProductCount;
            set => SetProperty(ref _totalProductCount, value);
        }

        private int _employeeCount;
        public int EmployeeCount
        {
            get => _employeeCount;
            set => SetProperty(ref _employeeCount, value);
        }

        private int _pendingCount;
        public int PendingCount
        {
            get => _pendingCount;
            set => SetProperty(ref _pendingCount, value);
        }

        private int _defectiveCount;
        public int DefectiveCount
        {
            get => _defectiveCount;
            set => SetProperty(ref _defectiveCount, value);
        }

        // Commands
        public ICommand OpenProductsViewCommand { get; }
        public ICommand OpenScanViewCommand { get; }
        public ICommand OpenManageViewCommand { get; }

        public HomeViewModel(string username, MainViewModel mainViewModel = null)
        {
            _dashboardRepository = new DashboardRepository();
            _mainViewModel = mainViewModel;
            Username = username;

            // Commands'ları initialize et
            OpenProductsViewCommand = new ViewModelCommand(param => NavigateToView("Product"));
            OpenScanViewCommand = new ViewModelCommand(param => NavigateToView("Scan"));
            OpenManageViewCommand = new ViewModelCommand(param => NavigateToView("Manage"));

            LoadAllData();
        }

        private void NavigateToView(string viewName)
        {
            if (_mainViewModel == null) return;

            switch (viewName)
            {
                case "Product":
                    _mainViewModel.ShowProductViewCommand.Execute(null);
                    break;
                case "Scan":
                    _mainViewModel.ShowScanViewCommand.Execute(null);
                    break;
                case "Manage":
                    _mainViewModel.ShowManageViewCommand.Execute(null);
                    break;
            }
        }

        private async void LoadAllData()
        {
            try
            {
                await Task.Run(() =>
                {
                    // Sadece sayılar çekiliyor
                    TotalProductCount = _dashboardRepository.GetTotalProductCount();
                    EmployeeCount = _dashboardRepository.GetEmployeeCount();
                    PendingCount = _dashboardRepository.GetPendingProductsCount(); // StatusID = 5 (Beklemede)
                    DefectiveCount = _dashboardRepository.GetDefectiveProductCount(); // StatusID = 2 (Arızalı)
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken bir hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Hata: {ex.Message}");
                // Hata durumunda varsayılan değerler atanıyor
                TotalProductCount = 0;
                EmployeeCount = 0;
                PendingCount = 0;
                DefectiveCount = 0;
            }
        }
    }
}