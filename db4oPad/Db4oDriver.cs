using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Gamlor.Db4oPad.GUI;
using Gamlor.Db4oPad.MetaInfo;
using LINQPad;
using LINQPad.Extensibility.DataContext;

namespace Gamlor.Db4oPad
{
    public class Db4oDriver : DynamicDataContextDriver
    {
        internal const string AssemblyLocation = "Db4oDriver.AssemblyLocationKey";
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
            get { return "db4o Driver Alpha"; }
        }

        public override string Author
        {
            get { return "Roman 'Gamlor' Stoffel"; }
        }

        public override List<ExplorerItem> GetSchemaAndBuildAssembly(IConnectionInfo cxInfo,
            AssemblyName assemblyToBuild,
            ref string nameSpace, ref string typeName)
        {
            using (var context = DatabaseContext.Create(
                OpenDB(cxInfo),
                assemblyToBuild))
            {
                cxInfo.SessionData[AssemblyLocation] = assemblyToBuild.CodeBase;
                nameSpace = context.MetaInfo.DataContext.Namespace;
                typeName = context.MetaInfo.DataContext.Name;
                return context.ListTypes().ToList();
            }
        }

        public override void TearDownContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager, object[] constructorArguments)
        {
            CurrentContext.CloseContext();
        }

        public override void InitializeContext(IConnectionInfo cxInfo,
            object context, QueryExecutionManager executionManager)
        {
            var assembly = LoadAssembly(cxInfo);
            var configurator = Configurator(cxInfo, assembly);

            var ctx = DatabaseContext.Create(OpenDB(cxInfo, configurator.Item1.Configure),configurator.Item2);
            CurrentContext.NewContext(ctx);
        }

        public override ICustomMemberProvider GetCustomDisplayMemberProvider(object objectToWrite)
        {
            return MemberProvider.Create(objectToWrite)
                .GetValue(()=>GetDefaultVisualisation(objectToWrite));
        }

        private ICustomMemberProvider GetDefaultVisualisation(object objectToWrite)
        {
            return base.GetCustomDisplayMemberProvider(objectToWrite);
        }

        private Tuple<DatabaseConfigurator,DatabaseMetaInfo> Configurator(IConnectionInfo cxInfo, Assembly assembly)
        {
            using (var tmpDb = OpenDB(cxInfo))
            {
                var meta = DatabaseMetaInfo.Create(tmpDb, assembly);
                return Tuple.Create(DatabaseConfigurator.Create(meta),meta);
            }
        }

        private Assembly LoadAssembly(IConnectionInfo cxInfo)
        {
            return Assembly.LoadFrom(GetAssemblyLocation(cxInfo));
        }

        private IEmbeddedObjectContainer OpenDB(IConnectionInfo cxInfo)
        {
            return OpenDB(cxInfo, c => { });
        }

        private IEmbeddedObjectContainer OpenDB(IConnectionInfo cxInfo,
            Action<IEmbeddedConfiguration> configurator)
        {
            var config = NewConfig();
            configurator(config);
            return Db4oEmbedded.OpenFile(config,cxInfo.CustomTypeInfo.CustomMetadataPath);
        }

        private IEmbeddedConfiguration NewConfig()
        {
            var config = Db4oEmbedded.NewConfiguration();
            config.File.ReadOnly = true;
            return config;
        }

        private string GetAssemblyLocation(IConnectionInfo cxInfo)
        {
            return (string)cxInfo.SessionData[AssemblyLocation];
        }

        public override IEnumerable<string> GetNamespacesToAdd()
        {
            return new[] {CodeGenerator.NameSpace};
        }



    }
}

