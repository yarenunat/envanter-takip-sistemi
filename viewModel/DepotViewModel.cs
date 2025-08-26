using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApp1.model;
using WpfApp1.Repositories;
using WpfApp1.view;

namespace WpfApp1.viewModel
{
    public class DepotViewModel : ViewModelBase
    {
        private const string AllLabel = "Tümü";
        private readonly IDepotRepository _repo;
        private bool _isLoading;

        public ObservableCollection<DepotItem> DepotItems { get; }
        public ObservableCollection<string> Categories { get; }
        public ObservableCollection<string> StatusFilters { get; }

        private DepotItem _selectedProduct;
        public DepotItem SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    ((RelayCommand)UpdateStatusCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value) && !_isLoading)
                {
                    ApplyFilters();
                }
            }
        }

        private string _selectedStatus;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value) && !_isLoading)
                {
                    ApplyFilters();
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand UpdateStatusCommand { get; }

        public DepotViewModel()
        {
            _repo = new DepotRepository();
            DepotItems = new ObservableCollection<DepotItem>();
            Categories = new ObservableCollection<string>();
            StatusFilters = new ObservableCollection<string>();

            RefreshCommand = new RelayCommand(_ => LoadInitialData());
            UpdateStatusCommand = new RelayCommand(UpdateStatus, CanUpdateStatus);

            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            try
            {
                _isLoading = true;
                await Task.Run(() =>
                {
                    var allCategories = _repo.GetAllCategories().ToList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Categories.Clear();
                        Categories.Add(AllLabel);
                        foreach (var cat in allCategories.Where(c => !string.IsNullOrWhiteSpace(c)))
                            Categories.Add(cat);
                        SelectedCategory = AllLabel;

                        StatusFilters.Clear();
                        StatusFilters.Add(AllLabel);
                        StatusFilters.Add("Mevcut");
                        StatusFilters.Add("Arızalı");
                        StatusFilters.Add("Hurda");
                        StatusFilters.Add("Beklemede");
                        SelectedStatus = AllLabel;
                    });
                });

                ApplyFilters();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Initialization error: {ex}");
                MessageBox.Show($"Veriler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ApplyFilters()
        {
            try
            {
                var filteredItems = _repo.GetDepotItems(
                    SelectedCategory == AllLabel ? null : SelectedCategory,
                    SelectedStatus == AllLabel ? null : SelectedStatus
                ).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DepotItems.Clear();
                    foreach (var item in filteredItems)
                    {
                        DepotItems.Add(item);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Filtreleme hatası: {ex.Message}");
                MessageBox.Show($"Filtreleme işlemi sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanUpdateStatus(object parameter)
        {
            return SelectedProduct != null;
        }

        private void UpdateStatus(object parameter)
        {
            if (SelectedProduct != null)
            {
                var updateDialog = new UpdateStatusView(SelectedProduct);
                if (updateDialog.ShowDialog() == true)
                {
                    LoadInitialData();
                }
            }
        }
    }
}