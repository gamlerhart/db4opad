using System.ComponentModel;
using System.IO;
using Gamlor.Db4oPad.Utils;
using LINQPad.Extensibility.DataContext;

namespace Gamlor.Db4oPad.GUI
{
    public class ConnectionViewModel : INotifyPropertyChanged
    {
        private readonly IConnectionInfo cxInfo;

        public ConnectionViewModel(IConnectionInfo cxInfo)
        {
            this.cxInfo = cxInfo;
            this.DatabasePath = cxInfo.CustomTypeInfo.CustomMetadataPath;
        }

        public string DatabasePath
        {
            get { return cxInfo.CustomTypeInfo.CustomMetadataPath; }
            set
            {
                cxInfo.CustomTypeInfo.CustomMetadataPath = value;
                PropertyChanged.Fire(this, () => DatabasePath);
                PropertyChanged.Fire(this, () => CanBeOpened);
            }
        }

        public bool Persist
        {
            get { return cxInfo.Persist; }
            set { cxInfo.Persist = value; }
        }

        public bool CanBeOpened
        {
            get { return null != DatabasePath && File.Exists(DatabasePath); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}