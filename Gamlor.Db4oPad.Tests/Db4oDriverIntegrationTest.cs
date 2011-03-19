using System;
using System.Collections;
using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
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
            CopyTestDB();
            var testInstance = new Db4oDriver();
            var connectionInfo = new Mock<IConnectionInfo>();
            var assembly = NewAssemblyName();
            var assemblyPath = assembly.CodeBase;
            connectionInfo.Setup(i => i.CustomTypeInfo.CustomMetadataPath)
                .Returns(() => Databasename);
            connectionInfo.Setup(i => i.SessionData[Db4oDriver.AssemblyLocation])
                .Returns(() => assemblyPath);
            var dummy = "";
            testInstance.GetSchemaAndBuildAssembly(connectionInfo.Object, assembly,
                ref dummy, ref dummy);
            testInstance.InitializeContext(connectionInfo.Object, null,null);
            try
            {
                var type = Assembly.LoadFrom(assemblyPath).GetType("LINQPad.User.Gamlor.Db4oPad.Tests.OtherData.MyData");
                var queryMethod = typeof (CurrentContext).GetMethod("Query").MakeGenericMethod(type);
                var result = (IEnumerable)queryMethod.Invoke(null, new object[0]);
                Assert.IsTrue(result.GetEnumerator().MoveNext());
                
            }finally
            {
                testInstance.TearDownContext(connectionInfo.Object, null, null, null);
            }

        }

        [Test]
        public void StoreAssembly()
        {
            var theName = NewAssemblyName();
            var assembly = AppDomain.CurrentDomain
                .DefineDynamicAssembly(theName, AssemblyBuilderAccess.RunAndSave,
                                       Path.GetDirectoryName(theName.CodeBase));

            assembly.Save(Path.GetFileName(theName.CodeBase));
        }

        private static AssemblyName NewAssemblyName()
        {
            var assemblyName = "TestAssembyl_" + Path.GetRandomFileName();
            var path = Path.Combine(Path.GetTempPath(), assemblyName);
            return new AssemblyName(Path.GetFileNameWithoutExtension(assemblyName))
                       {
                           CodeBase = path,

                           ProcessorArchitecture = ProcessorArchitecture.None,
                           VersionCompatibility = AssemblyVersionCompatibility.SameMachine

                       };
        }

        private void CopyTestDB()
        {
            File.Delete(Databasename);
            File.Copy("../../" + Databasename, Databasename);
        }

    }
}