using System;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Reflect.Net;
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

        [SetUp]
        public void Setup()
        {
            this.toTest = DynamicGeneratedTypesReflector.CreateInstance();
        }
        [Test]
        public void DoesResolveRuntimeName()
        {
            var runtimeType = CodeGenerator.NameSpace + ".MyType";
            var specialType = typeof(ClassWithoutFields);

            toTest.AddNewTypes(new[] { Tuple.Create(runtimeType, specialType) });


            var type = toTest.ForName(runtimeType);
            Assert.AreEqual(runtimeType, type.GetName());
            Assert.AreEqual(specialType, type.NewInstance().GetType());
            
        }
        [Test]
        public void ResolvesWithDBName()
        {
            var runtimeType = CodeGenerator.NameSpace + ".MyType";
            var specialType = typeof(ClassWithoutFields);

            toTest.AddNewTypes(new[] { Tuple.Create(runtimeType, specialType) });


            var type = toTest.ForClass(specialType);
            Assert.AreEqual(runtimeType, type.GetName());

        }
        [Test]
        public void FallBackIfTypeNotFound()
        {
            var runtimeType = ReflectPlatform.FullyQualifiedName(typeof(string));

            var type = toTest.ForName(runtimeType);
            Assert.AreEqual(runtimeType,type.GetName());
        }


        [Test]
        public void CanCloneItself()
        {
            var passedInstance = new object();
            var newInstance = toTest.DeepClone(passedInstance);
            Assert.AreNotSame(newInstance, toTest);
        }
    }

}