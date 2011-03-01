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

namespace Gamlor.Db4oPad.GUI
{
    /// <summary>
    /// Interaction logic for ConnectDialog.xaml
    /// </summary>
    public partial class ConnectDialog : Window
    {
        private IConnectionInfo contextInfo;

        public ConnectDialog(IConnectionInfo cxInfo)
        {
            DataContext = cxInfo.CustomTypeInfo;
            this.contextInfo = cxInfo;
            InitializeComponent();
        }

        private void BrowseAssembly(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
                          {
                              FileName = "",
                              DefaultExt = ".db4o",
                              Filter = "db4o databases (.db4o)|*.db4o;*.yap|All files (*.*)|*.*"
                          };
            var result = dialog.ShowDialog();

            if (result == true)
            {
                contextInfo.CustomTypeInfo.CustomMetadataPath = dialog.FileName;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
