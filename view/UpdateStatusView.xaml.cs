using System.Windows;
using WpfApp1.model;
using WpfApp1.viewModel;

namespace WpfApp1.view
{
    public partial class UpdateStatusView : Window
    {
        // Bu yapıcı metot, DepotViewModel'dan çağrıldığında çalışır.
        // DepotItem türünde bir argüman alır ve DataContext'i ayarlar.
        public UpdateStatusView(DepotItem product)
        {
            InitializeComponent();
            DataContext = new UpdateStatusViewModel(product);
        }
    }
}