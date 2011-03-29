using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestTypeGeneration : TypeGenerationBase
    {

        [Test]
        public void GenerateEmptyClass()
        {
            var metaInfo = TestMetaData.CreateEmptyClassMetaInfo();

            var type = GenerateSingle(metaInfo);
            Assert.AreEqual(metaInfo.Single().Name, type.Name);
        }
        [Test]
        public void IsInRightAssembly()
        {
            var metaInfo = TestMetaData.CreateEmptyClassMetaInfo();
            var assemblyName = new AssemblyName("Gamlor.Dynamic.Name.Of.This.Assembly")
                                   {
                                       CodeBase = Path.GetTempFileName(),
                                       Version = new Version(1, 0, 0, 1),
                                       CultureInfo = CultureInfo.InvariantCulture
                                   };
            var type = CodeGenerator.Create(metaInfo, assemblyName).Single(SingleNotObject);

            var generatedAssembly = type.Value.Assembly.GetName();
            Assert.AreEqual(assemblyName.Name, generatedAssembly.Name);
            Assert.AreEqual(assemblyName.Version, generatedAssembly.Version);
            Assert.AreEqual(assemblyName.CultureInfo, generatedAssembly.CultureInfo);
        }

        [Test]
        public void CanInstantiateClass()
        {
            var metaInfo = TestMetaData.CreateEmptyClassMetaInfo();

            var type = GenerateSingle(metaInfo);
            var instance = CreateInstance(type);
            Assert.NotNull(instance);
        }

        [Test]
        public void CanInstantiateGenericWithClass()
        {
            var metaInfo = TestMetaData.CreateEmptyClassMetaInfo();

            var type = GenerateSingle(metaInfo);
            var genericList = typeof(List<>).MakeGenericType(type);
            var list = genericList.GetConstructor(new Type[0]).Invoke(new object[0]);
        }

        [Test]
        public void CanHandleBuiltInType()
        {
            var metaInfo = new[] { KnownType.String };

            var type = GenerateSingle(metaInfo);
            Assert.AreEqual(typeof(string), type);
        }
        [Test]
        public void CanCreateTypeMultipleTimes()
        {
            var metaInfo = TestMetaData.CreateEmptyClassMetaInfo();

            var type1 = SingleNotObject(metaInfo);
            var type2 = SingleNotObject(metaInfo);
            Assert.NotNull(type1);
            Assert.NotNull(type2);
        }


        [Test]
        public void CanInstantiateClassWithSingleField()
        {
            var metaInfo = TestMetaData.CreateSingleFieldClass();
            var type = ExtractSingleFieldType(metaInfo);
            Assert.AreEqual(SingleFieldMeta(metaInfo).Name, type.Name);
            dynamic instance = CreateInstance(type);
            AssertFieldCanBeSet(instance, "newData");
        }
        [Test]
        public void CanInstantiateClassWithArrayFields()
        {
            var metaInfo = TestMetaData.CreateClassWithArrayField();
            var type = ExtractSingleFieldType(metaInfo);
            Assert.AreEqual(SingleFieldMeta(metaInfo).Name, type.Name);
            dynamic instance = CreateInstance(type);
            AssertFieldCanBeSet(instance, new []{"1","2"});
        }
        [Test]
        public void ArraysAreLowerBoundZero()
        {
            var arrayType = TestMetaData.CreateEmptyClassMetaInfo().Single();
            var metaInfo = SimpleClassDescription.Create(TestMetaData.SingleFieldType(),
                                                         f => TestMetaData.CreateArrayField(arrayType));
            var type = ExtractSingleFieldType(new[]{metaInfo});
            var fieldType= type.GetField("data").FieldType;
            Assert.IsTrue(fieldType.AssemblyQualifiedName.Contains("[]"));
        }

        [Test]
        public void CanAccessAutoProperty()
        {
            var metaInfo = TestMetaData.CreateSingleAutoPropertyClass();
            var type = ExtractSingleFieldType(metaInfo);
            Assert.AreEqual(SingleFieldMeta(metaInfo).Name, type.Name);
            dynamic instance = CreateInstance(type);
            instance.Data = "data";
            Assert.AreEqual("data", instance.Data);
        }

        [Test]
        public void CanInstantiateCircularType()
        {
            var metaInfo = CircularType();

            var type = GenerateSingle(metaInfo);
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
        public void SaneGenericsName()
        {
            var metaInfo = GenericType();

            var type = ExtractSingleFieldType(metaInfo);
            Assert.AreEqual(TestMetaData.SingleFieldClassName + "_1_String", type.Name);
        }
        [Test]
        public void CanInstanteSubClass()
        {
            var metaInfo = SubClassType();

            var type = ExtractTypeByName(metaInfo,"SubClass");
            dynamic instance = CreateInstance(type);
            AssertFieldCanBeSet(instance, "dataOnParent");
            instance.subField = 2;
            Assert.AreEqual(2, instance.subField);
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
            var metaInfo = TestMetaData.CreateSingleFieldClass();
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
            var metaInfo = TestMetaData.CreateEmptyClassMetaInfo();

            var name = NewName();
            var infos = CodeGenerator.Create(metaInfo, name);
            Assert.IsTrue(File.Exists(name.CodeBase));

        }
        [Test]
        public void CanAccessExistingArrayType()
        {
            var knownType = KnownType.Create(typeof(ClassWithFields));
            var knownTypeArray = ArrayDescription.Create(knownType,1);
            var metaInfo = new[] { knownType, knownTypeArray };

            var result = CodeGenerator.Create(metaInfo,
                Assembly.GetAssembly(typeof(TestTypeGeneration)));
            var arrayType = result.Types[knownTypeArray];
            Assert.NotNull(arrayType);
        }

        private IEnumerable<ITypeDescription> SubClassType()
        {
            var baseClasses = TestMetaData.CreateSingleFieldClass();
            var intType = KnownType.Create(typeof(int));
            var baseClass = baseClasses.Single(b=>!b.KnowsType.HasValue);
            var subType = SimpleClassDescription.Create(
                TypeName.Create("ANamespace.SubClass", TestMetaData.AssemblyName), baseClass,
                f => TestMetaData.CreateField("subField", intType));
            return baseClasses.Concat(new[] {intType, subType});
        }

        private void AssertFieldCanBeSet(dynamic instance, dynamic fieldInstance)
        {
            instance.data = fieldInstance;
            Assert.AreEqual(fieldInstance, instance.data);
        }

        private Type GenerateSingle(IEnumerable<ITypeDescription> metaInfo)
        {
            return SingleNotObject(metaInfo).Value;
        }

        private object CreateInstance(Type type)
        {
            var constructor = type.GetConstructors().Single();
            return constructor.Invoke(new object[0]);
        }
        private IEnumerable<ITypeDescription> CreateSingleIntFieldClass()
        {
            var stringType = KnownType.Create(typeof(int));
            var type = SimpleClassDescription.Create(TestMetaData.SingleFieldType(),
                                                             f => TestMetaData.CreateField(stringType));
            return new[] { type, stringType };
        }


        private static IEnumerable<ITypeDescription> CircularType()
        {
            var type = SimpleClassDescription.Create(TestMetaData.SingleFieldType(),
                                                      TestMetaData.CreateField);
            return new ITypeDescription[] { type };
        }

        private static IEnumerable<ITypeDescription> TwoGenericInstances()
        {
            var stringList = KnownType.Create(typeof(List<string>));
            var stringType = typeof(string);
            var stringGenericArgs = GenericArg(stringType);
            var stringInstance = SimpleClassDescription.Create(TypeName.Create(TestMetaData.SingleFieldTypeName, TestMetaData.AssemblyName, stringGenericArgs),
                                                     f => TestMetaData.CreateField(stringList));

            var intList = KnownType.Create(typeof(List<int>));
            var intType = typeof(int);
            var intGenericArgs = GenericArg(intType);
            var intInstance = SimpleClassDescription.Create(TypeName.Create(TestMetaData.SingleFieldTypeName, TestMetaData.AssemblyName, intGenericArgs),
                                                     f => TestMetaData.CreateField(intList));
            return new[] { stringInstance, intInstance, stringList, intList };
        }

        private static Type ExtractSingleFieldType(IEnumerable<ITypeDescription> metaInfo)
        {
            return ExtractTypeByName(metaInfo, "SingleField");
        }

        private static Type ExtractTypeByName(IEnumerable<ITypeDescription> metaInfo, string name)
        {
            return CodeGenerator.Create(metaInfo, NewName()).Where(t => t.Key.Name.Contains(name)).Single().Value;
        }

        private static ITypeDescription SingleFieldMeta(IEnumerable<ITypeDescription> metaInfo)
        {
            return metaInfo.Where(t => t.Name.Contains("SingleField")).Single();
        }

        private KeyValuePair<ITypeDescription, Type> SingleNotObject(IEnumerable<ITypeDescription> metaInfo)
        {
            return NewTestInstance(metaInfo).Single(SingleNotObject);
        }

        private bool SingleNotObject(KeyValuePair<ITypeDescription, Type> t)
        {
            return t.Key != KnownType.Object && t.Key != KnownType.Array;
        }
    }
}