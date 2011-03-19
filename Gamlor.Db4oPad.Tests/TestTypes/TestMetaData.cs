using System.Collections.Generic;
using System.Reflection;
using Gamlor.Db4oPad.MetaInfo;

namespace Gamlor.Db4oPad.Tests.TestTypes
{
    public static class TestMetaData
    {
        public const string AssemblyName = "AAssembly";
        public const string SingleFieldTypeName = "ANamespace.SingleField";


        internal static IEnumerable<ITypeDescription> CreateEmptyClassMetaInfo()
        {
            return new[]{SimpleClassDescription.Create(TypeName.Create("ANamespace.EmptyClass", AssemblyName),
                                                 (f) => new SimpleFieldDescription[0])};
        }
    }
}