using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gamlor.Db4oPad.GUI;
using LINQPad.Extensibility.DataContext;

namespace Gamlor.Db4oPad
{
    public class Db4oDriver : DynamicDataContextDriver
    {
        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            return Path.GetFileName(cxInfo.CustomTypeInfo.CustomMetadataPath);
        }

        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            var dialog = new ConnectDialog(cxInfo);
            var result = dialog.ShowDialog();
            return result.HasValue && result.Value;
        }

        public override string Name
        {
            get { return "db4o Driver"; }
        }

        public override string Author
        {
            get { return "Roman Stoffel"; }
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
        {
            var item = new ExplorerItem("fun", ExplorerItemKind.QueryableObject, ExplorerIcon.Table);
            return new List<ExplorerItem>() { item };
        }


    }
}