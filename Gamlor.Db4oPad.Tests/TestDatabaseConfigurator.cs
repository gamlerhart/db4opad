using System.IO;
using System.Linq;
using System.Reflection;
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
            var db = MemoryDBForTests.NewDB(
                config =>
                    {
                        var meatInfo =
                            DatabaseMetaInfo.Create(
                                TestMetaData.CreateEmptyClassMetaInfo(), AssemblyName());

                        var toTest = new DatabaseConfigurator(meatInfo);

                        toTest.Configure(config);
                    });
            var typeInfo = db.Ext().Reflector().ForName(dynamicClass.TypeName.FullName);
            Assert.AreEqual(dynamicClass.TypeName.FullName, typeInfo.GetName());
        }
    }
}