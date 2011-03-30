using System.IO;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Reflect;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using Moq;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class TestDatabaseConfigurator
    {
        private AssemblyName AssemblyName()
        {
            return new AssemblyName("test")
                       {
                           CodeBase = Path.GetTempFileName()
                       };
        }

        [Test]
        public void SetsReflector()
        {
            var configMock = new Mock<IEmbeddedConfiguration>();
            var commonConfig = new Mock<ICommonConfiguration>();
            configMock.Setup(c => c.Common).Returns(commonConfig.Object);

            var meatInfo = DatabaseMetaInfo.Create(new ITypeDescription[0], AssemblyName());

            var toTest = new DatabaseConfigurator(meatInfo);

            toTest.Configure(configMock.Object);

            commonConfig.Verify(c => c.ReflectWith(It.IsAny<IReflector>()));
        }

        [Test]
        public void HasConfiguredReflector()
        {
            var meta = TestMetaData.CreateEmptyClassMetaInfo();
            var dynamicClass = meta.Single();
            var metaInfo =
                DatabaseMetaInfo.Create(
                    TestMetaData.CreateEmptyClassMetaInfo(), AssemblyName());
            var db = Configure(metaInfo);
            var typeInfo = db.Ext().Reflector().ForName(dynamicClass.TypeName.FullName);
            Assert.AreEqual(dynamicClass.TypeName.FullName, typeInfo.GetName());
        }

        [Test]
        public void DoesNotAddArrayTypes()
        {
            var meta = TestMetaData.CreateClassWithArrayField();
            var db = Configure(DatabaseMetaInfo.Create(meta, TestUtils.NewName()));
            Assert.NotNull(db);
        }
        [Test]
        public void DoesAddPartialGenerics()
        {
            var meta = TestMetaData.CreateClassListGenericOf(TestMetaData.CreateEmptyClassMetaInfo().Single());
            var db = Configure(DatabaseMetaInfo.Create(meta, TestUtils.NewName()));
            var typeWithGenericField = meta.First();
            var fieldType = typeWithGenericField.Fields.Single();
            var typeInfo = db.Ext().Reflector().ForName(fieldType.Type.TypeName.FullName);
            Assert.NotNull(typeInfo);
            Assert.IsTrue(typeInfo.GetName().StartsWith("System.Collections.Generic.List`1[[ANamespace"));
        }

        private IObjectContainer Configure(DatabaseMetaInfo info)
        {
            return MemoryDBForTests.NewDB(
                config =>
                {
                    var toTest = new DatabaseConfigurator(info);

                    toTest.Configure(config);
                });
        }
    }
}