using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfApp1.model;
using WpfApp1.Repositories;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Collections.Generic;

namespace WpfApp1.viewModel
{
    public class UpdateStatusViewModel : ViewModelBase
    {
        private readonly DepotRepository _repo;
        private readonly DepotItem _product;

        public ObservableCollection<string> StatusNames { get; }
        public ObservableCollection<EmployeeModel> Employees { get; }

        private string _selectedStatus;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    ((RelayCommand)UpdateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        private EmployeeModel _selectedEmployee;
        public EmployeeModel SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (SetProperty(ref _selectedEmployee, value))
                {
                    ((RelayCommand)UpdateCommand)?.RaiseCanExecuteChanged();
                    ((RelayCommand)AssignCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand UpdateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AssignCommand { get; }

        public UpdateStatusViewModel(DepotItem product)
        {
            _repo = new DepotRepository();
            _product = product;

            // Durum listesini al ve "Zimmetli" ekle
            var statusList = new List<string>(_repo.GetStatusNames());
            if (!statusList.Contains("Zimmetli"))
            {
                statusList.Add("Zimmetli");
            }

            StatusNames = new ObservableCollection<string>(statusList);
            Employees = new ObservableCollection<EmployeeModel>(_repo.GetEmployees());

            SelectedStatus = _product.StatusName;

            UpdateCommand = new RelayCommand(ExecuteUpdate, CanExecuteUpdate);
            CancelCommand = new RelayCommand(ExecuteCancel);
            AssignCommand = new RelayCommand(ExecuteAssign, CanExecuteAssign);
        }

        private bool CanExecuteUpdate(object parameter)
        {
            return !string.IsNullOrEmpty(SelectedStatus);
        }

        private bool CanExecuteAssign(object parameter)
        {
            return SelectedEmployee != null;
        }

        private void ExecuteUpdate(object parameter)
        {
            try
            {
                _repo.UpdateProductStatus(_product.ProductID, SelectedStatus);

                MessageBox.Show("Ürün durumu başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Durum güncelleme hatası: {ex.Message}");
                MessageBox.Show($"Durum güncelleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAssign(object parameter)
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Lütfen bir personel seçin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ** ZİMMETLEME UYARI KONTROLÜ BAŞLANGICI **
                string[] uyariDurumlari = { "Arızalı", "Hurda", "Mevcut" };
                if (uyariDurumlari.Contains(_product.StatusName))
                {
                    string mesaj = $"Bu ürünün mevcut durumu '{_product.StatusName}'.\n" +
                                   "Zimmetleme işlemi sonucunda ürünün durumu 'Zimmetli' olarak güncellenecek ve depo listesinden kaldırılacaktır.\n" +
                                   "Yine de devam etmek istiyor musunuz?";

                    var result = MessageBox.Show(mesaj, "Uyarı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No)
                    {
                        // Kullanıcı işlemi iptal etti
                        return;
                    }
                }
                // ** ZİMMETLEME UYARI KONTROLÜ SONU **

                // Ürünü zimmetle
                _repo.AssignProduct(_product.ProductID, SelectedEmployee.EmployeeId);

                // Durumu otomatik olarak "Zimmetli" yap
                _repo.UpdateProductStatus(_product.ProductID, "Zimmetli");

                MessageBox.Show($"Ürün {SelectedEmployee.FullName} adlı personele zimmetlendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                // Pencereyi kapat
                Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Zimmetleme hatası: {ex.Message}");
                MessageBox.Show($"Zimmetleme hatası: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel(object parameter)
        {
            if (parameter is Window window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }
    }
}