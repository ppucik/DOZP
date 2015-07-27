using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

using Comdat.DOZP.Core;
using Comdat.DOZP.Process;

namespace Comdat.DOZP.OCR
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

        private string OcrFolderPath
        {
            get { return this.OcrFolderPathTextBox.Text; }
            set { this.OcrFolderPathTextBox.Text = value; }
        }

        private string FineReaderPath
        {
            get { return this.FineReaderPathTextBox.Text; }
            set { this.FineReaderPathTextBox.Text = value; }
        }

        #endregion

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Institution institution = DozpController.GetInstitution();
            this.CatalogueComboBox.ItemsSource = (institution != null ? institution.Catalogues : null);
        }

        private void ScanFolderButton_Click(object sender, RoutedEventArgs e)
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

        private void OcrFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.Description = this.OcrFolderPathLabel.Content.ToString();
                dlg.SelectedPath = this.OcrFolderPath;

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    OcrFolderPath = dlg.SelectedPath.ToString();
                }
            }
        }

        private void FineReaderButton_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.Title = this.FineReaderPathLabel.Content.ToString();
                dlg.InitialDirectory = "C:\\";
                dlg.Filter = "EXE Files|*.exe";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FineReaderPath = dlg.FileName;
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string message = String.Empty;

            if (String.IsNullOrEmpty(OcrFolderPath))
            {
                message = "Není zadaná cesta pro ukládání zpracovaných dokumentů.";
                MessageBox.Show(message, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (String.IsNullOrEmpty(FineReaderPath))
            {
                message = "Není vybraná cesta k ABBYY FineReader aplikaci.";
                MessageBox.Show(message, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.Cursor = Cursors.Wait;

            try
            {
                if (!Directory.Exists(ScanFolderPath))
                    Directory.CreateDirectory(ScanFolderPath);

                if (!Directory.Exists(OcrFolderPath))
                    Directory.CreateDirectory(OcrFolderPath);

                Properties.Settings.Default.OcrFolderPath = this.OcrFolderPath;
                Properties.Settings.Default.FineReaderPath = this.FineReaderPath;
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
