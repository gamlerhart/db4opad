using System;
using Db4objects.Db4o.Reflect;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using Moq;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestDynamicGeneratedTypesReflector
    {
        private DynamicGeneratedTypesReflector toTest;
        private Mock<IReflector> innerReflector;

        [SetUp]
        public void Setup()
        {
            this.innerReflector = new Mock<IReflector>();
            this.toTest = DynamicGeneratedTypesReflector.CreateInstance(innerReflector.Object);
        }
        [Test]
        public void DoesResolveRuntimeName()
        {
            var reflectClass = new Mock<IReflectClass>().Object;
            var runtimeType = CodeGenerator.NameSpace + ".MyType";
            var specialType = typeof(ClassWithoutFields);

            toTest.AddNewTypes(new[] { Tuple.Create(runtimeType, specialType) });


            innerReflector.Setup(r => r.ForClass(specialType)).Returns(reflectClass);

            var type = toTest.ForName(runtimeType);
            Assert.AreEqual(reflectClass, type);
            innerReflector.Verify(r => r.ForName(runtimeType), Times.Never());
            innerReflector.Verify(r => r.ForClass(specialType));
        }
        [Test]
        public void FallBackIfTypeNotFound()
        {
            var runtimeType = CodeGenerator.NameSpace + ".MyType";

            var type = toTest.ForName(runtimeType);
            innerReflector.Verify(r => r.ForName(runtimeType));
        }


        [Test]
        public void CanCloneItself()
        {
            var passedInstance = new object();

            var newInstance = toTest.DeepClone(passedInstance);
            innerReflector.Verify(r => r.DeepClone(passedInstance));

            Assert.AreNotSame(newInstance, toTest);
        }

        [Test]
        public void DelegatesArrays()
        {
            toTest.Array();
            innerReflector.Verify(r => r.Array());
        }
        [Test]
        public void DelegatesConfiguration()
        {
            var configMock = new Mock<IReflectorConfiguration>();
            toTest.Configuration(configMock.Object);
            innerReflector.Verify(r => r.Configuration(configMock.Object));
        }
        [Test]
        public void DelegatesForClass()
        {
            var stringType = typeof(string);

            toTest.ForClass(stringType);
            innerReflector.Verify(r => r.ForClass(stringType));
        }
        [Test]
        public void DelegatesForName()
        {
            var stringType = typeof(string).Name;

            toTest.ForName(stringType);
            innerReflector.Verify(r => r.ForName(stringType));
        }
        [Test]
        public void DelegatesForObject()
        {
            var obj = "";

            toTest.ForObject(obj);
            innerReflector.Verify(r => r.ForObject(obj));
        }
        [Test]
        public void DelegatesIsCollection()
        {
            var reflectClass = new Mock<IReflectClass>().Object;

            toTest.IsCollection(reflectClass);
            innerReflector.Verify(r => r.IsCollection(reflectClass));
        }
        [Test]
        public void DelegatesSetParent()
        {
            var parent = new Mock<IReflector>().Object;

            toTest.SetParent(parent);
            innerReflector.Verify(r => r.SetParent(parent));
        }
    }

}