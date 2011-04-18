using System;
using System.Configuration.Assemblies;
using System.IO;
using System.Reflection;
using Db4objects.Db4o;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Utils;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    public static class TestUtils
    {
        internal static Func<ITypeDescription, Type> FindNothingTypeResolver =
            t => { Assert.Fail("Don't expect this call");
                       return null;
            };
        public static void WithTestContext(Action action)
        {
            var db = MemoryDBForTests.NewDB();
            WithTestContext(db,TestTypeResolver(), action);
        }

        internal static void WithTestContext(IObjectContainer db,TypeResolver resolver, Action action)
        {
            using (var ctx = DatabaseContext.Create(db, NewName(), resolver))
            {
                CurrentContext.NewContext(ctx);
                action();
            }
        }

        public static AssemblyName NewName()
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


        /// <summary>
        /// For some tests we need to simulate the case where we don't have the required assemblies.
        /// </summary>
        /// <returns></returns>
        internal static TypeResolver TestTypeResolver()
        {
            var defaultResolver = MetaDataReader.DefaultTypeResolver();
            return n => n.FullName.StartsWith("Gamlor.Db4oPad.Tests")
                ? Maybe<Type>.Empty
                : defaultResolver(n);
        }
        internal static TypeResolver DefaultResolver()
        {
            return MetaDataReader.DefaultTypeResolver();
        }


        internal static void CopyTestDB(string dbName)
        {
            File.Delete(dbName);
            File.Copy("../../" + dbName, dbName);
        }
    }
}