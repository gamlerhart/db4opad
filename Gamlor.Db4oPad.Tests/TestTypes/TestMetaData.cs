using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gamlor.Db4oPad.MetaInfo;

namespace Gamlor.Db4oPad.Tests.TestTypes
{
    internal static class TestMetaData
    {
        public const string AssemblyName = "AAssembly";
        public const string SingleFieldTypeName = "ANamespace.SingleField";
        public readonly static ITypeDescription StringType = new SystemType(typeof(string));


        internal static IEnumerable<ITypeDescription> CreateEmptyClassMetaInfo()
        {
            return new[]{SimpleClassDescription.Create(TypeName.Create("ANamespace.EmptyClass", AssemblyName),
                                                 (f) => new SimpleFieldDescription[0])};
        }

        internal static IEnumerable<ITypeDescription> CreateSingleFieldClass()
        {
            var type = SimpleClassDescription.Create(SingleFieldType(),
                                                             f => CreateField(StringType));
            return new[] { type, StringType };
        }
        internal static IEnumerable<ITypeDescription> CreateSingleAutoPropertyClass()
        {
            var type = SimpleClassDescription.Create(SingleFieldType(),
                                                             f => CreateField(AutoPropertyName.Name(), StringType));
            return new[] { type, StringType };
        }

        internal static TypeName SingleFieldType()
        {
            return TypeName.Create(SingleFieldTypeName, AssemblyName);
        }
        internal static IEnumerable<SimpleFieldDescription> CreateField(string fieldName,
            ITypeDescription type)
        {
            return new[] { SimpleFieldDescription.Create(fieldName, type) };
        }
        internal static IEnumerable<SimpleFieldDescription> CreateField(ITypeDescription type)
        {
            return CreateField("data", type);
        }

        private class AutoPropertyName
        {
            public string Data { get; set; }
            public static string Name()
            {
                return typeof (AutoPropertyName)
                    .GetFields(BindingFlags.NonPublic|BindingFlags.Instance)
                    .Single().Name;
            }
        }
    }
}