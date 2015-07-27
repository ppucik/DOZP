using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

using Comdat.DOZP.Core;
using Comdat.DOZP.Process;

using TwainDotNet;
using TwainDotNet.TwainNative;
using TwainDotNet.Wpf;

namespace Comdat.DOZP.Scan
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        #region Private members

        #endregion

        #region Constructors

        public OptionsDialog()
        {
            InitializeComponent();
        }

        public OptionsDialog(Window parent)
            : this()
        {
            this.Owner = parent;
        }

        #endregion

        #region Properties

        private string ScanFolderPath
        {
            get { return this.ScanFolderPathTextBox.Text; }
            set { this.ScanFolderPathTextBox.Text = value; }
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Institution institution = DozpController.GetInstitution();
            this.CatalogueComboBox.ItemsSource = (institution != null ? institution.Catalogues : null);

            Twain twain = new Twain(new WpfWindowMessageHook(this));
            this.ScannerSourceNameComboBox.ItemsSource = twain.SourceNames;

            this.AppLoggingButton.IsEnabled = File.Exists(Logger.FileName);
        }

        private void ScanFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dlg.Description = this.ScanFolderPathLabel.Content.ToString();
                    dlg.SelectedPath = this.ScanFolderPath;

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ScanFolderPath = dlg.SelectedPath.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AppLoggingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(Logger.FileName))
                {
                    System.Diagnostics.Process.Start(Logger.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string message = String.Empty;

            if (String.IsNullOrEmpty(ScanFolderPath))
            {
                message = "Není zadaná cesta pro ukládání naskenovaných souborů.";
                MessageBox.Show(message, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                if (!Directory.Exists(ScanFolderPath))
                    Directory.CreateDirectory(ScanFolderPath);

                Properties.Settings.Default.ScanFolderPath = this.ScanFolderPath;
                Properties.Settings.Default.Save();

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
            }
            finally
            {
                this.Cursor = null;
            }
        }

        #endregion
    }
}
