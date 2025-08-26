using System.Windows.Controls;
using WpfApp1.viewModel;

namespace WpfApp1.view
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();

            string username = System.Windows.Application.Current.Properties["Username"]?.ToString() ?? "admin";
            DataContext = new HomeViewModel(username);
        }
    }
}
