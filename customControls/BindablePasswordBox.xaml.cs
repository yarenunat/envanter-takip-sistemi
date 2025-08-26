using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1.CustomControls
{
    /// <summary>
    /// Interaction logic for BindablePasswordBox.xaml
    /// </summary>
    public partial class BindablePasswordBox : UserControl
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(SecureString), typeof(BindablePasswordBox));

        private PasswordBox _passwordBox;

        public SecureString Password
        {
            get { return (SecureString)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public BindablePasswordBox()
        {
            InitializeComponent();
            _passwordBox = this.FindName("txtPassword") as PasswordBox;
            if (_passwordBox != null)
            {
                _passwordBox.PasswordChanged += OnPasswordChanged;
            }
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                Password = passwordBox.SecurePassword;
            }
        }
    }
}