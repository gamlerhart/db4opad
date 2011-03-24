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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LINQPad.Extensibility.DataContext;
using Microsoft.Win32;

namespace Gamlor.Db4oPad.GUI
{
    /// <summary>
    /// Interaction logic for ConnectDialog.xaml
    /// </summary>
    public partial class ConnectDialog : Window
    {
        private ConnectionViewModel model;

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
            var dialog = new Microsoft.Win32.OpenFileDialog
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

        private string AssemblyPaths(OpenFileDialog dialog)
        {
            return dialog.FileNames.Aggregate("", (e, n) => e + n + Environment.NewLine);
        }
    }
}
