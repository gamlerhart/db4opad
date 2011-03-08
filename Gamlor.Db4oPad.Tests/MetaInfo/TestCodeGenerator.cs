using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Gamlor.Db4oPad.MetaInfo;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestCodeGenerator
    {
        private const string SingleFieldTypeName = "ANamespace.SingleField";
        private const string AssemblyName = "AAssembly";

        [Test]
        public void GenerateEmptyClass()
        {
            var metaInfo = CreateEmptyClassMetaInfo();

            var type = GenerateSingle(metaInfo);
            Assert.AreEqual(metaInfo.Single().Name, type.Name);
        }
        [Test]
        public void IsInRightAssembly()
        {
            var metaInfo = CreateEmptyClassMetaInfo();
            var assemblyName = new AssemblyName("Gamlor.Dynamic.Name.Of.This.Assembly");
            assemblyName.Version = new Version(1,0,0,1);
            assemblyName.CultureInfo = CultureInfo.InvariantCulture;
            var type = CodeGenerator.Create(metaInfo, assemblyName).Single();

            var generatedAssembly = type.Value.Assembly.GetName();
            Assert.AreEqual(assemblyName.Name, generatedAssembly.Name);
            Assert.AreEqual(assemblyName.Version, generatedAssembly.Version);
            Assert.AreEqual(assemblyName.CultureInfo, generatedAssembly.CultureInfo);
        }

        [Test]
        public void CanInstantiateClass()
        {
            var metaInfo = CreateEmptyClassMetaInfo();

            var type = GenerateSingle(metaInfo);
            var instance = CreateInstance(type);
            Assert.NotNull(instance);
        }


        [Test]
        public void CanInstantiateGenericWithClass()
        {
            var metaInfo = CreateEmptyClassMetaInfo();

            var type = GenerateSingle(metaInfo);
            var genericList = typeof(List<>).MakeGenericType(type);
            var list = genericList.GetConstructor(new Type[0]).Invoke(new object[0]);
        }

        [Test]
        public void CanHandleBuiltInType()
        {
            var metaInfo = new[] { new SystemType(typeof(string)) };

            var type = GenerateSingle(metaInfo);
            Assert.AreEqual(typeof(string), type);
        }
        [Test]
        public void CanCreateTypeMultipleTimes()
        {
            var metaInfo = CreateEmptyClassMetaInfo();

            var type1 = NewTestInstance(metaInfo).Single();
            var type2 = NewTestInstance(metaInfo).Single();
            Assert.NotNull(type1);
            Assert.NotNull(type2);
        }


        [Test]
        public void CanInstantiateClassWithSingleField()
        {
            var metaInfo = CreateSingleFieldClass();
            var type = ExtractSingleFieldType(metaInfo);
            Assert.AreEqual(SingleFieldMeta(metaInfo).Name, type.Name);
            dynamic instance = CreateInstance(type);
            AssertFieldCanBeSet(instance, "newData");
        }

        [Test]
        public void CanInstantiateCircularType()
        {
            var metaInfo = CircularType();

            var type = NewTestInstance(metaInfo).Single().Value;
            Assert.AreEqual(metaInfo.Single().Name, type.Name);
            dynamic instance = CreateInstance(type);
            dynamic fieldInstance = CreateInstance(type);
            AssertFieldCanBeSet(instance, fieldInstance);
        }
        [Test]
        public void CanInstantiateTypeWithGeneric()
        {
            var metaInfo = TypeWithGenericList();

            var type = ExtractSingleFieldType(metaInfo);
            Assert.AreEqual(SingleFieldMeta(metaInfo).Name, type.Name);
            dynamic instance = CreateInstance(type);
            var fieldInstance = new List<string>();
            AssertFieldCanBeSet(instance, fieldInstance);
        }
        [Test]
        public void CanInstantiateGeneric()
        {
            var metaInfo = GenericType();

            var type = ExtractSingleFieldType(metaInfo);
            dynamic instance = CreateInstance(type);
            var fieldInstance = new List<string>();
            AssertFieldCanBeSet(instance, fieldInstance);
        }
        [Test]
        public void CanInstantiateDifferentGenericInstances()
        {
            var metaInfo = TwoGenericInstances();

            var types = NewTestInstance(metaInfo);
            var genericTypes = types.Where(f => f.Key.Name.Contains("SingleField"));
            Assert.AreEqual(2, genericTypes.Count());

            foreach (var type in genericTypes.Select(kv => kv.Value))
            {
                dynamic instance = CreateInstance(type);
                Assert.NotNull(instance);
            }
        }

        [Test]
        public void HasPublicGetterForLowerCaseBeginners()
        {
            var metaInfo = CreateSingleFieldClass();
            var type = ExtractSingleFieldType(metaInfo);
            Assert.AreEqual(SingleFieldMeta(metaInfo).Name, type.Name);
            dynamic instance = CreateInstance(type);
            instance.Data = "newData";
            Assert.AreEqual("newData", instance.Data);
        }
        [Test]
        public void GetterSetterWorkAlsoForInt()
        {
            var metaInfo = CreateSingleIntFieldClass();
            var type = ExtractSingleFieldType(metaInfo);
            Assert.AreEqual(SingleFieldMeta(metaInfo).Name, type.Name);
            dynamic instance = CreateInstance(type);
            instance.Data = 1;
            Assert.AreEqual(1, instance.Data);
        }
        [Test]
        public void HasAssembly()
        {
            var metaInfo = CreateEmptyClassMetaInfo();

            var name = NewName();
            name.CodeBase = Path.GetTempFileName();
            var infos = CodeGenerator.Create(metaInfo, name);
            Assert.IsTrue(File.Exists(name.CodeBase));

        }

        private void AssertFieldCanBeSet(dynamic instance, dynamic fieldInstance)
        {
            instance.data = fieldInstance;
            Assert.AreEqual(fieldInstance, instance.data);
        }

        private Type GenerateSingle(IEnumerable<ITypeDescription> metaInfo)
        {
            return NewTestInstance(metaInfo).Single().Value;
        }

        private object CreateInstance(Type type)
        {
            var constructor = type.GetConstructors().Single();
            return constructor.Invoke(new object[0]);
        }

        private IEnumerable<ITypeDescription> CreateEmptyClassMetaInfo()
        {
            return new[]{SimpleClassDescription.Create(TypeName.Create("ANamespace.EmptyClass", AssemblyName),
                                                 (f) => new SimpleFieldDescription[0])};
        }
        private IEnumerable<ITypeDescription> CreateSingleFieldClass()
        {
            var stringType = new SystemType(typeof(string));
            var type = SimpleClassDescription.Create(SingleFieldType(),
                                                             f => CreateField(stringType));
            return new ITypeDescription[] { type, stringType };
        }
        private IEnumerable<ITypeDescription> CreateSingleIntFieldClass()
        {
            var stringType = new SystemType(typeof(int));
            var type = SimpleClassDescription.Create(SingleFieldType(),
                                                             f => CreateField(stringType));
            return new ITypeDescription[] { type, stringType };
        }


        private static IEnumerable<ITypeDescription> CircularType()
        {
            var type = SimpleClassDescription.Create(SingleFieldType(),
                                                      CreateField);
            return new ITypeDescription[] { type };
        }
        private static IEnumerable<ITypeDescription> TypeWithGenericList()
        {
            var stringList = new SystemType(typeof(List<string>));
            var type = SimpleClassDescription.Create(SingleFieldType(),
                                                     f => CreateField(stringList));
            return new ITypeDescription[] { type, stringList };
        }

        private static IEnumerable<ITypeDescription> GenericType()
        {
            var stringList = new SystemType(typeof(List<string>));
            var stringType = typeof(string);
            var genericArguments = GenericArg(stringType);
            var type = SimpleClassDescription.Create(TypeName.Create(SingleFieldTypeName, AssemblyName, genericArguments),
                                                     f => CreateField(stringList));
            return new ITypeDescription[] { type, stringList };
        }
        private static IEnumerable<ITypeDescription> TwoGenericInstances()
        {
            var stringList = new SystemType(typeof(List<string>));
            var stringType = typeof(string);
            var stringGenericArgs = GenericArg(stringType);
            var stringInstance = SimpleClassDescription.Create(TypeName.Create(SingleFieldTypeName, AssemblyName, stringGenericArgs),
                                                     f => CreateField(stringList));

            var intList = new SystemType(typeof(List<int>));
            var intType = typeof(int);
            var intGenericArgs = GenericArg(intType);
            var intInstance = SimpleClassDescription.Create(TypeName.Create(SingleFieldTypeName, AssemblyName, intGenericArgs),
                                                     f => CreateField(intList));
            return new ITypeDescription[] { stringInstance, intInstance, stringList, intList };
        }

        private static IEnumerable<TypeName> GenericArg(Type intType)
        {
            return new[] { TypeName.Create(intType.FullName, intType.Assembly.GetName().Name) };
        }

        private static TypeName SingleFieldType()
        {
            return TypeName.Create(SingleFieldTypeName, AssemblyName);
        }
        private static IEnumerable<SimpleFieldDescription> CreateField(ITypeDescription stringType)
        {
            return new[] { SimpleFieldDescription.Create("data", stringType) };
        }

        private static Type ExtractSingleFieldType(IEnumerable<ITypeDescription> metaInfo)
        {
            return CodeGenerator.Create(metaInfo, NewName()).Where(t => t.Key.Name.Contains("SingleField")).Single().Value;
        }

        private static ITypeDescription SingleFieldMeta(IEnumerable<ITypeDescription> metaInfo)
        {
            return metaInfo.Where(t => t.Name.Contains("SingleField")).Single();
        }

        private static AssemblyName NewName()
        {
            return new AssemblyName("Gamlor.DynamicAssembly");
        }

        private CodeGenerator.Result NewTestInstance(IEnumerable<ITypeDescription> metaInfo)
        {
            return CodeGenerator.Create(metaInfo, NewName());
        }
    }

}