using System;
using System.Collections.Generic;
using System.Linq;
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
            var theType = TestMetaData.CreateGenericType();
            Assert.AreEqual("TheType_1_Int32",theType.Name);
        }
        [Test]
        public void GenericNameWithTwoParams()
        {
            var complex = TestMetaData.CreateGenericType(
                TypeName.Create("System.Int32", "mscorelib"),TypeName.Create("System.Int64", "mscorelib"));

            Assert.AreEqual("TheType_2_Int32_Int64",
                complex.Name);
        }
        [Test]
        public void CascadedGenericsName()
        {
            var theType = TestMetaData.CreateGenericType();
            var complex = TestMetaData.CreateGenericType(
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
            var theName = TypeName.Create("System.Int32", "mscorelib", new TypeName[0], 0);
            var innerType = SimpleClassDescription.Create(theName, f => new SimpleFieldDescription[0]);
            var arrayType = ArrayDescription.Create(innerType, 1);
            Assert.IsTrue(arrayType.IsArray);
        }
        [Test]
        public void ArrayCanCreateItself()
        {
            TypeName theName = TypeName.Create("System.Int32", "mscorelib", new TypeName[0], 0);
            var innerType = SimpleClassDescription.Create(theName, f => new SimpleFieldDescription[0]);
            var arrayType = ArrayDescription.Create(innerType, 1);
            var intArray = arrayType.TryResolveType(t => typeof (int));
            Assert.AreEqual(typeof(int[]),intArray.Value);
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
            var t1 = TestMetaData.CreateGenericType();
            var t2 = TestMetaData.CreateGenericType();

            Assert.AreEqual(t1, t2);
        }
        [Test]
        public void SystemTypeEquals()
        {
            var t1 = KnownType.Create(typeof(string));
            var t2 = KnownType.Create(typeof(string));

            Assert.AreEqual(t1, t2);
        }

        [Test]
        public void ObjectIsRecursive()
        {
            var type = KnownType.Object;
            Assert.IsFalse(type.BaseClass.HasValue);
        }

        [Test]
        public void GeneratedIsBusinessType()
        {
            var theType = TestMetaData.CreateGenericType();
            Assert.IsTrue(theType.IsBusinessEntity);
        }
        [Test]
        public void ArrayIsNotBusinessType()
        {
            var theType = ArrayDescription.Create(TestMetaData.CreateGenericType(), 1);
            Assert.IsFalse(theType.IsBusinessEntity);
        }
        [Test]
        public void SystemTypeIsNotBusinessType()
        {
            var theType = KnownType.String;
            Assert.IsFalse(theType.IsBusinessEntity);
        }
        [Test]
        public void Db4oTypeIsNotBusinessType()
        {
            var theType = KnownType.Create(typeof(ActivatableList<string>));
            Assert.IsFalse(theType.IsBusinessEntity);
        }
        [Test]
        public void KnownTypeIsBusinessType()
        {
            var theType = KnownType.Create(typeof(ClassWithFields));
            Assert.IsTrue(theType.IsBusinessEntity);
        }
        [Test]
        public void KnownTypeReturnsPublicFields()
        {
            var theType = KnownType.Create(typeof(ClassWithPublicField));
            Assert.AreEqual(1,theType.Fields.Count());
        }
        [Test]
        public void ResolvesGenericParameters()
        {
            var otherType = TestMetaData.CreateEmptyClassMetaInfo();
            var theType = KnownType.Create(typeof(System.Collections.Generic.List<>),otherType);
            var resovledType = theType.TryResolveType(t => typeof (string));
            Assert.AreEqual(typeof(System.Collections.Generic.List<string>), resovledType.Value);
            Assert.IsTrue(theType.TypeName.FullName.StartsWith("System.Collections.Generic.List"));
        }
        [Test]
        public void KnownTypeReturnsPropertyFields()
        {
            var theType = KnownType.Create(typeof(ClassWithProperty));
            Assert.AreEqual(1, theType.Fields.Count());
        }
        [Test]
        public void NameOfGenericInstance()
        {
            var listType = KnownType.Create(typeof(List<>), new[] { KnownType.String });

            Assert.IsTrue(listType.TypeName.FullName.StartsWith("System.Collections.Generic.List`1["));
        }

        private ITypeDescription CreateArrayType()
        {
            TypeName theName = TypeName.Create("System.Int32", "mscorelib", new TypeName[0], 0);
            var innerType = SimpleClassDescription.Create(theName, f => new SimpleFieldDescription[0]);
            return ArrayDescription.Create(innerType, 1);
        }

        // Here we're listing test-types. We're not using it. Therefore we can suppress the warnings.
#pragma warning disable 169
#pragma warning disable 649
        // ReSharper disable UnusedMember.Local
        class ClassWithPublicField
        {
            public string theField;
        }
        class ClassWithProperty
        {
            private string theField;

            public string TheField
            {
                get { return theField; }
            }
        }
        // ReSharper restore UnusedMember.Local
#pragma warning restore 649
#pragma warning restore 169
    }
}