using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Klient
{
    /// <summary>
    /// Logika interakcji dla klasy App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            SynchronousTCPClient.Init();
        }

        protected override void OnExit(System.Windows.ExitEventArgs e)
        {
            SynchronousTCPClient.Close();
        }
    }
}
