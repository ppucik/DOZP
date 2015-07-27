using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

using Comdat.DOZP.Core;

namespace Comdat.DOZP.Scan
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //#if DEBUG
            //            AuthController.UserIdentity.Authenticate("admin", "comdat2389");  
            //#else
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            LoginDialog login = new LoginDialog();
            if (login.ShowDialog() ?? false)
            {
                MainWindow window = new MainWindow();
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                Current.MainWindow = window;
                window.Show();
            }
            else
            {
                Current.Shutdown();
            }
            //#endif
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, Comdat.DOZP.Scan.Properties.Resources.ApplicationName);
            Logger.Log(e.Exception.Message);
            
            e.Handled = true;
        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
    }
}
