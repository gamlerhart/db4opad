using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    public abstract class TypeGenerationBase
    {

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
        internal static IEnumerable<ITypeDescription> TypeWithGenericList()
        {
            var stringList = new SystemType(typeof(List<string>));
            var type = SimpleClassDescription.Create(SingleFieldType(),
                                                     f => CreateField(stringList));
            return new ITypeDescription[] { type, stringList };
        }
        internal static IEnumerable<ITypeDescription> GenericType()
        {
            var stringList = new SystemType(typeof(List<string>));
            var stringType = typeof(string);
            var genericArguments = GenericArg(stringType);
            var type = SimpleClassDescription.Create(TypeName.Create(TestMetaData.SingleFieldTypeName,
                TestMetaData.AssemblyName, genericArguments),
                                                     f => CreateField(stringList));
            return new ITypeDescription[] { type, stringList };
        }

        internal static IEnumerable<TypeName> GenericArg(Type intType)
        {
            return new[] { TypeName.Create(intType.FullName, intType.Assembly.GetName().Name) };
        }
        internal static IEnumerable<SimpleFieldDescription> CreateField(ITypeDescription stringType)
        {
            return CreateField("data", stringType);
        }
        internal static IEnumerable<SimpleFieldDescription> CreateField(string fieldName, 
            ITypeDescription stringType)
        {
            return new[] { SimpleFieldDescription.Create(fieldName, stringType) };
        }

        internal static TypeName SingleFieldType()
        {
            return TypeName.Create(TestMetaData.SingleFieldTypeName, TestMetaData.AssemblyName);
        }

    }
}