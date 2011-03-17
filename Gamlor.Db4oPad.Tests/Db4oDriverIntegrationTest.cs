using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Reflection;
using LINQPad.Extensibility.DataContext;
using Moq;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    public class Db4oDriverIntegrationTest
    {
        private const string Databasename = "testDB.db4o";

        [Test]
        public void CanQuery()
        {
            var testInstance = new Db4oDriver();
            var connectionInfo = new Mock<IConnectionInfo>();
            var assemblyPath = Path.GetTempFileName();
            connectionInfo.Setup(i => i.CustomTypeInfo.CustomMetadataPath)
                .Returns(() => assemblyPath);
            connectionInfo.Setup(i => i.SessionData[Db4oDriver.AssemblyLocation])
                .Returns(() => assemblyPath);
            var dummy = "";
            testInstance.GetSchemaAndBuildAssembly(connectionInfo.Object, NewAssemblyName(),
                ref dummy, ref dummy);
            testInstance.InitializeContext(connectionInfo.Object, null,null);
            try
            {
                CurrentContext.Query<object>();
                
            }finally
            {
                testInstance.TearDownContext(connectionInfo.Object, null, null, null);
            }

        }

        private AssemblyName NewAssemblyName()
        {
            return new AssemblyName("TempAssembly")
                       {
                           CodeBase = Path.GetTempFileName(),
                           CultureInfo = CultureInfo.InvariantCulture,

                           ProcessorArchitecture = ProcessorArchitecture.MSIL,
                           VersionCompatibility = AssemblyVersionCompatibility.SameProcess

                       };
        }

        private void CopyTestDB()
        {
            File.Delete(Databasename);
            File.Copy("../../" + Databasename, Databasename);
        }

    }
}