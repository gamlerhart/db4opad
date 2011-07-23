using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gamlor.Db4oPad.MetaInfo;

namespace Gamlor.Db4oPad.Tests.TestTypes
{
    internal static class TestMetaData
    {
        public const string AssemblyName = "AAssembly";
        public const string SingleFieldClassName = "SingleField";
        public const string Namespace = "ANamespace";
        public const string SubNamespace = "OtherNamespace";
        public const string EmptyClassName =  "EmptyClass";
        public const string SingleFieldTypeName = Namespace+"." + SingleFieldClassName;
        public const string FieldName = "data";
        public readonly static ITypeDescription StringType = KnownType.String;


        internal static IEnumerable<ITypeDescription> CreateEmptyClassMetaInfo()
        {
            return new[] { CreateEmptySimpleClass( Namespace+"."+EmptyClassName) };
        }
        internal static IEnumerable<ITypeDescription> CreateNameConflicMetaInfo()
        {
            return new[]{CreateEmptySimpleClass( Namespace+"."+EmptyClassName),
                CreateEmptySimpleClass("ANamespace.OtherNamespace.EmptyClass")};
        }

        private static SimpleClassDescription CreateEmptySimpleClass(string nameSpaceAndName)
        {
            return SimpleClassDescription.Create(TypeName.Create(nameSpaceAndName, AssemblyName),
                                                 (f) => new SimpleFieldDescription[0]);
        }

        internal static IEnumerable<ITypeDescription> CreateSingleFieldClass()
        {
            return CreateSingleFieldClass(StringType);
        }
        internal static IEnumerable<ITypeDescription> CreateSingleFieldClass(ITypeDescription fieldType)
        {
            var type = SimpleClassDescription.Create(SingleFieldType(),
                                                             f => CreateField(fieldType));
            return new[] { type, StringType };
        }

        public static IEnumerable<ITypeDescription> CreateClassWithArrayField()
        {
            var type = SimpleClassDescription.Create(SingleFieldType(),
                                                             f => CreateArrayField(StringType));
            return new[] { type, StringType };
        }
        public static IEnumerable<ITypeDescription> CreateClassListGenericOf(ITypeDescription typeOfLists)
        {
            var listType = KnownType.Create(typeof (List<>), new[]{typeOfLists});
            var type = SimpleClassDescription.Create(SingleFieldType(),
                                                             f => CreateField(listType));
            return new[] { type, listType, typeOfLists };
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
        internal static IEnumerable<SimpleFieldDescription> CreateArrayField(ITypeDescription type)
        {
            var arrayType = ArrayDescription.Create(type, 1);
            return new[] { SimpleFieldDescription.Create(FieldName, arrayType) };
        }
        internal static IEnumerable<SimpleFieldDescription> CreateField(ITypeDescription type)
        {
            return CreateField(FieldName, type);
        }

        internal static SimpleClassDescription CreateGenericType()
        {
            TypeName genericArg = TypeName.Create("System.Int32", "mscorelib");
            return CreateGenericType(genericArg);
        }

        internal static SimpleClassDescription CreateGenericType(params TypeName[] genericArg)
        {
            TypeName theName = TypeName.Create("ANamespace.TheType", "TheAssembly", genericArg);
            return SimpleClassDescription.Create(theName, f => new SimpleFieldDescription[0]);
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