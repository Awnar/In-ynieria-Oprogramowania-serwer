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

namespace Klient
{
    /// <summary>
    /// Logika interakcji dla klasy Login.xaml
    /// </summary>
    public partial class Login : Window
    {

        public Login()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var answer = SynchronousTCPClient.Login(login.Text, pass.Password);
                if (answer != null)
                {
                    error.Visibility = Visibility.Visible;
                    error.Content = answer;
                }
                else
                {
                    new MainWindow().Show();
                    Close();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Błąd z połączeniem, proszę spróbować zalogować się za jakiś czas.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            error.Visibility = Visibility.Visible;
            error.Content = SynchronousTCPClient.Register(login.Text, pass.Password);
        }
    }
}
