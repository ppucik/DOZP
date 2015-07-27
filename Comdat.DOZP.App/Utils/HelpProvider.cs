using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Comdat.DOZP.App
{
    public static class HelpProvider
    {
        public static readonly DependencyProperty HelpStringProperty = DependencyProperty.RegisterAttached("HelpString", typeof(string), typeof(HelpProvider));

        static HelpProvider()
        {
            CommandManager.RegisterClassCommandBinding(
                typeof(FrameworkElement),
                new CommandBinding(
                    ApplicationCommands.Help,
                    new ExecutedRoutedEventHandler(Executed),
                    new CanExecuteRoutedEventHandler(CanExecute)));
        }

        static private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            FrameworkElement senderElement = (sender as FrameworkElement);

            if (HelpProvider.GetHelpString(senderElement) != null)
                e.CanExecute = true;
        }

        static private void Executed(object sender, ExecutedRoutedEventArgs e)
        {
            FrameworkElement source = (sender as FrameworkElement);

            if (source != null)
            {
                string helpString = HelpProvider.GetHelpString(source);

                if (!String.IsNullOrEmpty(helpString))
                {
                    if (helpString.EndsWith(".aspx") || helpString.EndsWith(".pdf"))
                    {
#if DEBUG
                        System.Diagnostics.Process.Start(String.Format(@"http://localhost:12623/Help/{0}", helpString));
#else
                        System.Diagnostics.Process.Start(String.Format("{0}/Help/{1}", Properties.Settings.Default.AppWebsiteUrl, helpString));
#endif
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(String.Format("Nápověda: {0}", helpString), "Nápověda");
                    }
                }
            }
        }

        public static string GetHelpString(DependencyObject obj)
        {
            return (string)obj.GetValue(HelpStringProperty);
        }

        public static void SetHelpString(DependencyObject obj, string value)
        {
            obj.SetValue(HelpStringProperty, value);
        }
    }
}
