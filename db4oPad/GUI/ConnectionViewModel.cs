using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
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

        public bool WriteAccess
        {
            get
            {
                return LinqPadConfigUtils.HasWriteAccess(cxInfo);
            }
            set
            {
                cxInfo.DriverData.Element(LinqPadConfigUtils.WriteAccessFlag)
                    .AsMaybe()
                    .Handle(() => cxInfo.DriverData.Add(new XElement(LinqPadConfigUtils.WriteAccessFlag, value)))
                    .Apply(e => e.SetValue(value));
            }
        }

        public string AssemblyPath
        {
            get { return cxInfo.CustomTypeInfo.CustomAssemblyPath; }
            set
            {
                cxInfo.CustomTypeInfo.CustomAssemblyPath = value;
                PropertyChanged.Fire(this, () => AssemblyPath);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal const string AssemblyLocation = "Db4oDriver.AssemblyLocationKey";
    }
}