using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.IO;
using Db4objects.Db4o.Reflect;
using Gamlor.Db4oPad.MetaInfo;
using Moq;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class TestDatabaseConfigurator
    {
        [Test]
        public void AddsAliases()
        {
            var configMock = new Mock<IEmbeddedConfiguration>();
            var commonConfig = new Mock<ICommonConfiguration>();
            configMock.Setup(c => c.Common).Returns(commonConfig.Object);

            var addedAliases = new List<IAlias>();

            commonConfig.Setup(c => c.AddAlias(It.IsAny<IAlias>()))
                .Callback((IAlias a) => addedAliases.Add(a));

            var metaClass = SimpleClassDescription.Create(TypeName.Create("Test.SimpleType", "Simple.Assembly"));
            var meatInfo = DatabaseMetaInfo.Create(new[] { metaClass },
                AssemblyName());

            var toTest = new DatabaseConfigurator(meatInfo);

            toTest.Configure(configMock.Object);

            Assert.AreEqual(1, addedAliases.Count);
        }

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
    }

}