using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Shapes;

using Comdat.DOZP.Process;

namespace Comdat.DOZP.OCR
{
    /// <summary>
    /// Interaction logic for LoginDialog.xaml
    /// </summary>
    public partial class LoginDialog : Window
    {
        #region Constructors

        public LoginDialog()
        {
            InitializeComponent();

#if DEBUG
            this.UserNameTextBox.Text = "admin";
            this.PasswordTextBox.Password = "comdat2389";
#endif
        }

        #endregion

        #region Properties

        internal string UserName
        {
            get
            {
                return this.UserNameTextBox.Text.ToLower();
            }
        }

        internal string Password
        {
            get
            {
                return this.PasswordTextBox.Password;
            }
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                this.Title = String.Format("DOZP-Zpracování v{0}.{1}.{2} [Build {3}]", version.Major, version.Minor, version.Revision, version.Build);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(UserName))
            {
                MessageBox.Show("Není zadáno jméno.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (String.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Není zadáno heslo.", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;
            this.OkButton.IsEnabled = false;

            try
            {
                if (AuthController.UserIdentity.Authenticate(UserName, Password))
                {
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Neplatné uživatelské jméno neho heslo!", this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
            finally
            {
                this.Cursor = null;
                this.OkButton.IsEnabled = true;
            }
        }

        #endregion
    }
}
