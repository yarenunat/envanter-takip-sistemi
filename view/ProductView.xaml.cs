using System.Windows;
using System.Windows.Controls;
using WpfApp1.viewModel;

namespace WpfApp1.view
{
    public partial class ProductView : UserControl
    {
        public ProductView()
        {
            InitializeComponent();
            DataContext = new ProductViewModel();
        }

        private void BarcodeSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = DataContext as ProductViewModel;
            vm?.SearchByBarcode(BarcodeSearchBox.Text);
        }

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ProductViewModel;
            vm?.LoadProducts();
        }
    }
}
