using System.Windows.Controls;
using WpfApp1.viewModel;

namespace WpfApp1.view
{
    public partial class InOutLogView : UserControl
    {
        public InOutLogView()
        {
            InitializeComponent();
            DataContext = new InOutLogViewModel();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
