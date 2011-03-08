using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Gamlor.Db4oPad.GUI;
using Gamlor.Db4oPad.MetaInfo;
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

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo,
            AssemblyName assemblyToBuild,
            ref string nameSpace, ref string typeName)
        {
            using (var db = Db4oEmbedded.OpenFile(cxInfo.CustomTypeInfo.CustomMetadataPath))
            {
                var ctx =  DatabaseContext.Create(db, assemblyToBuild);
                return ctx.ListTypes().ToList();
            }
        }

        public override void TearDownContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager, object[] constructorArguments)
        {
            Console.Out.WriteLine("Down");
        }

        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            Console.Out.WriteLine("");
        }

        public override IEnumerable<string> GetNamespacesToAdd()
        {
            return new[] {CodeGenerator.NameSpace};
        }

    }


}

namespace LINQPad.User
{
    public class Cheat
    {
        
    }
    public class TypedDataContext
    {
        public IQueryable<Cheat> Cheat { get { return new[] {new Cheat()}.AsQueryable(); } }   
    }

}