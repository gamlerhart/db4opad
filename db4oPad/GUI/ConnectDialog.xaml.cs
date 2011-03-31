using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace Gamlor.Db4oPad.GUI
{
    /// <summary>
    /// Interaction logic for ConnectDialog.xaml
    /// </summary>
    public partial class ConnectDialog : Window
    {
        private readonly ConnectionViewModel model;

        public ConnectDialog(ConnectionViewModel model)
        {
            DataContext = model;
            this.model = model;
            InitializeComponent();
        }

        private void BrowseDatabase(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
                          {
                              CheckFileExists = true,
                              FileName = "",
                              DefaultExt = ".db4o",
                              Filter = "db4o databases (.db4o)|*.db4o;*.yap|All files (*.*)|*.*"
                          };
            var result = dialog.ShowDialog();

            if (result == true)
            {
                model.DatabasePath = dialog.FileName;
            }
        }

        private void OpenDB(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BrowseAssembly(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                FileName = "",
                DefaultExt = ".db4o",
                Filter = "Assemblies (.dll,.exe)|*.dll;*.exe|Any Assembly (*.*)|*.*"
            };
            var result = dialog.ShowDialog();

            if (result == true)
            {
                model.AssemblyPath = AssemblyPaths(dialog);
            }
        }

        private static string AssemblyPaths(FileDialog dialog)
        {
            return dialog.FileNames.Aggregate("", (e, n) => e + n + Environment.NewLine);
        }
    }
}
