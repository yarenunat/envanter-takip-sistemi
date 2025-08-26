using System.Windows;
using System.Windows.Controls;
using WpfApp1.model;
using WpfApp1.viewModel;

namespace WpfApp1.view
{
    public partial class EmployeeView : UserControl
    {
        public EmployeeView()
        {
            InitializeComponent();
            DataContext = new EmployeeViewModel();
        }

        private void EmployeeExpander_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is Expander expander &&
                expander.DataContext is EmployeeModel employee &&
                DataContext is EmployeeViewModel vm)
            {
                vm.LoadAssignedItems(employee);
            }
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EmployeeViewModel vm)
            {
                vm.ClearFilters();
            }
        }
    }
}
