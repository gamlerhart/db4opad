using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Gamlor.Db4oPad.GUI;
using LINQPad.Extensibility.DataContext;
using Moq;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.GUI
{
    [TestFixture]
    public class TestConnectionViewModel
    {
        private Mock<IConnectionInfo> mock;
        private ConnectionViewModel toTest;

        [SetUp]
        public void Setup()
        {
            this.mock = new Mock<IConnectionInfo>();
            mock.Setup(c => c.Persist).Returns(true);
            mock.Setup(c => c.CustomTypeInfo.CustomMetadataPath).Returns("");
            mock.Setup(c => c.CustomTypeInfo.CustomAssemblyPath).Returns("");
            this.toTest = new ConnectionViewModel(this.mock.Object);
        }

        [Test]
        public void UsesExistingMetaPath()
        {
            mock.Setup(c => c.CustomTypeInfo.CustomMetadataPath).Returns("ExistingPath");
            Assert.AreEqual("ExistingPath", NewTestInstance().DatabasePath);
        }

        [Test]
        public void UsesExistingWriteAccessFlag()
        {
            var valueFunction = new XElement("root",
                new XElement(ConnectionViewModel.WriteAccessFlag,"false"));
            mock.Setup(c => c.DriverData).Returns(valueFunction);
            Assert.AreEqual(false, NewTestInstance().WriteAccess);
        }
        [Test]
        public void CanSetAndGetWriteAccessFlag()
        {
            var valueFunction = new XElement("root",
                new XElement(ConnectionViewModel.WriteAccessFlag, "false"));
            mock.Setup(c => c.DriverData).Returns(valueFunction);
            NewTestInstance().WriteAccess = true;
            Assert.AreEqual(true, NewTestInstance().WriteAccess);
            Assert.AreEqual("true", valueFunction.Element(ConnectionViewModel.WriteAccessFlag).Value);
        }
        [Test]
        public void EmptyDataEqualsNoWriteAcces()
        {
            mock.Setup(c => c.DriverData).Returns(new XElement("root"));
            Assert.AreEqual(false, NewTestInstance().WriteAccess);
        }

        [Test]
        public void WritesBackPath()
        {
            toTest.DatabasePath = "NewPath";
            mock.VerifySet(c => c.CustomTypeInfo.CustomMetadataPath="NewPath");
        }
        [Test]
        public void UsesExistingPersists()
        {
            mock.Setup(c => c.Persist).Returns(true);
            Assert.IsTrue(NewTestInstance().Persist);
        }
        [Test]
        public void WritesBackPersists()
        {
            toTest.Persist = false;
            mock.VerifySet(c => c.Persist = false);
        }
        [Test]
        public void FireEventOnSet()
        {
            var fireCount = 0;
            toTest.PropertyChanged += (sender, args) =>
                                          {
                                              fireCount++;
                                          }; 
            toTest.DatabasePath = "NewPath";
            Assert.AreEqual(2, fireCount);
        }
        [Test]
        public void EmptyPathCannotBeOpened()
        {
            mock.Setup(c => c.CustomTypeInfo.CustomMetadataPath).Returns("");
            Assert.IsFalse(NewTestInstance().CanBeOpened);
        }
        [Test]
        public void NullPathCannotBeOpened()
        {
            mock.Setup(c => c.CustomTypeInfo.CustomMetadataPath).Returns((string)null);
            Assert.IsFalse(NewTestInstance().CanBeOpened);
        }
        [Test]
        public void NotExistingPath()
        {
            var notExisting = Path.GetRandomFileName();
            mock.Setup(c => c.CustomTypeInfo.CustomMetadataPath).Returns(notExisting);
            Assert.IsFalse(NewTestInstance().CanBeOpened);
        }
        [Test]
        public void RealPathCanBeOpened()
        {
            var existing = Path.GetTempFileName();
            mock.Setup(c => c.CustomTypeInfo.CustomMetadataPath).Returns(existing);
            Assert.IsTrue(NewTestInstance().CanBeOpened);
        }
        [Test]
        public void HasAssemblyPath()
        {
            mock.Setup(c => c.CustomTypeInfo.CustomAssemblyPath).Returns("AssemblyPath");
            Assert.AreEqual("AssemblyPath",NewTestInstance().AssemblyPath);
        }
        [Test]
        public void CanSetAssemblyPath()
        {
            toTest.AssemblyPath = "AssemblyPath";
            mock.VerifySet(c => c.CustomTypeInfo.CustomAssemblyPath = "AssemblyPath");
        }
        [Test]
        public void SettingAssembliesFiresEvent()
        {
            var wasFired = false;
            toTest.PropertyChanged += (sender, args) => wasFired = true;
            toTest.AssemblyPath = "AssemblyPath";
            Assert.IsTrue(wasFired);
        }

        private ConnectionViewModel NewTestInstance()
        {
            return new ConnectionViewModel(this.mock.Object);
        }
    }
}