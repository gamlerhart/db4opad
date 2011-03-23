using System;
using System.Configuration.Assemblies;
using System.IO;
using System.Reflection;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.Tests
{
    public static class TestUtils
    {
        public static void WithTestContext(Action action)
        {
            var db = MemoryDBForTests.NewDB();
            using (var ctx = DatabaseContext.Create(db, new AssemblyName("test")
            {
                CodeBase = Path.GetTempFileName()
            }))
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
    }
}