using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gamlor.Db4oPad.MetaInfo;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    public abstract class TypeGenerationBase
    {
        internal const string AssemblyName = "AAssembly";

        internal static IEnumerable<ITypeDescription> CreateEmptyClassMetaInfo()
        {
            return new[]{SimpleClassDescription.Create(TypeName.Create("ANamespace.EmptyClass", AssemblyName),
                                                 (f) => new SimpleFieldDescription[0])};
        }

        internal static AssemblyName NewName()
        {
            return new AssemblyName("Gamlor.DynamicAssembly")
                       {
                           CodeBase = Path.GetTempFileName()
                       };
        }

        internal CodeGenerator.Result NewTestInstance(IEnumerable<ITypeDescription> metaInfo)
        {
            return CodeGenerator.Create(metaInfo, NewName());
        }

    }
}