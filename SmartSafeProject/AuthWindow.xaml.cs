using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmartSafeProject
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        string password = "admin";

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            if(PasswordTextBox.Password == password)
            {
                MainWindow window = new MainWindow();
                window.Owner = this;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                this.Hide();
                window.ShowDialog();
                this.Show();
            }
        }
    }
}
