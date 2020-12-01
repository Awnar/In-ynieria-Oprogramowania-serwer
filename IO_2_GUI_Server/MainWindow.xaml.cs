using IO_2_lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace IO_2_GUI_Server
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Logger.Info += ReceiveLog;
        }

        private void StartServerBtn_Click(object sender, RoutedEventArgs e)
        {
            //StartServerBtn. = true;
            int portFromTextBox = int.Parse(ServerPortTextBox.Text);
            StartServerBtn.Content = "Serwer już działa";
            StartServerBtn.IsEnabled = false;
            ServerPortTextBox.IsEnabled = false;
            Task task = new Task(() => OdpalSerwer(portFromTextBox));
            task.Start();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ReceiveLog(string content)
        {
            Console.WriteLine(content);
            this.Dispatcher.Invoke(() => {
                LogsFromServerTextBox.Text += $"{DateTime.Now.ToString("HH:mm:ss")} >> {content}\n";
            });
            
        }

        private void OdpalSerwer(int port)
        {
            var serwer = new Serwer(port: port);
            serwer.Start();
        }
    }
}
