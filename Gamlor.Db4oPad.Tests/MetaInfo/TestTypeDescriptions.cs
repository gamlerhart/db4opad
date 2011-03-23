using System;
using Db4objects.Db4o.Collections;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestTypeDescriptions
    {
        [Test]
        public void GenericName()
        {
            var theType = CreateGenericType();
            Assert.AreEqual("TheType_1_Int32",theType.Name);
        }
        [Test]
        public void GenericNameWithTwoParams()
        {
            var complex = CreateGenericType(
                TypeName.Create("System.Int32", "mscorelib"),TypeName.Create("System.Int64", "mscorelib"));

            Assert.AreEqual("TheType_2_Int32_Int64",
                complex.Name);
        }
        [Test]
        public void CascadedGenericsName()
        {
            var theType = CreateGenericType();
            var complex = CreateGenericType(
                TypeName.Create("System.Int32", "mscorelib"), theType.TypeName);

            Assert.AreEqual("TheType_2_Int32_TheType_1_Int32",
                complex.Name);
        }

        [Test]
        public void ThrowIfArrayType()
        {
            TypeName theName = TypeName.Create("System.Int32", "mscorelib",new TypeName[0],1);
            Assert.Throws<ArgumentException>(()=>SimpleClassDescription.Create(theName, f => new SimpleFieldDescription[0]));
            
        }
        [Test]
        public void ArrayType()
        {
            TypeName theName = TypeName.Create("System.Int32", "mscorelib", new TypeName[0], 0);
            var innerType = SimpleClassDescription.Create(theName, f => new SimpleFieldDescription[0]);
            var arrayType = ArrayDescription.Create(innerType, 1);
            Assert.IsTrue(arrayType.IsArray);
            Assert.AreEqual(innerType,arrayType.ArrayOf.Value);

        }
        [Test]
        public void ArrayEquals()
        {
            var arrayType1 = CreateArrayType();
            var arrayType2 = CreateArrayType();

            Assert.AreEqual(arrayType1, arrayType2);
        }
        [Test]
        public void GenericEquals()
        {
            var t1 = CreateGenericType();
            var t2 = CreateGenericType();

            Assert.AreEqual(t1, t2);
        }
        [Test]
        public void SystemTypeEquals()
        {
            var t1 = new KnownTypes(typeof(string));
            var t2 = new KnownTypes(typeof(string));

            Assert.AreEqual(t1, t2);
        }

        [Test]
        public void ObjectIsRecursive()
        {
            var type = KnownTypes.Object;
            Assert.AreEqual(type, type.BaseClass);
        }

        [Test]
        public void GeneratedIsBusinessType()
        {
            var theType = CreateGenericType();
            Assert.IsTrue(theType.IsBusinessEntity);
        }
        [Test]
        public void ArrayIsNotBusinessType()
        {
            var theType = ArrayDescription.Create(CreateGenericType(), 1);
            Assert.IsFalse(theType.IsBusinessEntity);
        }
        [Test]
        public void SystemTypeIsNotBusinessType()
        {
            var theType = new KnownTypes(typeof(string));
            Assert.IsFalse(theType.IsBusinessEntity);
        }
        [Test]
        public void Db4oTypeIsNotBusinessType()
        {
            var theType = new KnownTypes(typeof(ActivatableList<string>));
            Assert.IsFalse(theType.IsBusinessEntity);
        }
        [Test]
        public void KnownTypeIsBusinessType()
        {
            var theType = new KnownTypes(typeof(ClassWithFields));
            Assert.IsTrue(theType.IsBusinessEntity);
        }
        [Test]
        public void ToStringPrintsName()
        {
            var theType = TypeName.Create("System.Int32", "mscorelib");
            Assert.AreEqual("System.Int32",theType.ToString());
        }

        private SimpleClassDescription CreateGenericType()
        {
            TypeName genericArg = TypeName.Create("System.Int32", "mscorelib");
            return CreateGenericType(genericArg);
        }

        private SimpleClassDescription CreateGenericType(params TypeName[] genericArg)
        {
            TypeName theName = TypeName.Create("TheType", "TheAssembly", genericArg);
            return SimpleClassDescription.Create(theName, f => new SimpleFieldDescription[0]);
        }

        private ITypeDescription CreateArrayType()
        {
            TypeName theName = TypeName.Create("System.Int32", "mscorelib", new TypeName[0], 0);
            var innerType = SimpleClassDescription.Create(theName, f => new SimpleFieldDescription[0]);
            return ArrayDescription.Create(innerType, 1);
        }
    }
}