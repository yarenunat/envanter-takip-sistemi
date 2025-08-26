using System.Windows.Controls;
using WpfApp1.viewModel;

namespace WpfApp1.view
{
    public partial class DepotView : UserControl
    {
        public DepotView()
        {
            InitializeComponent();
            DataContext = new DepotViewModel();
        }
    }
}
