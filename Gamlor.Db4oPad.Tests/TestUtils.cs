using System.Configuration.Assemblies;
using System.IO;
using System.Reflection;

namespace Gamlor.Db4oPad.Tests
{
    public static class TestUtils
    {
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
    }
}