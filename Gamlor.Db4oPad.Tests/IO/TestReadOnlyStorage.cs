using System;
using System.IO;
using Db4objects.Db4o.IO;
using Gamlor.Db4oPad.IO;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.IO
{

    public abstract class AbstractIOTestCase
    {
        protected static readonly byte[] existingData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        internal ReadOnlyStorage toTest;

        [SetUp]
        public void Setup()
        {
            this.toTest = new ReadOnlyStorage();
            AdditionalSetup(toTest);
        }

        internal virtual void AdditionalSetup(ReadOnlyStorage readOnlyStorage)
        {
        }

        protected static string NewFileWithBytes()
        {
            var file = Path.GetTempFileName();
            File.WriteAllBytes(file, existingData);
            return file;
        }
    }

    [TestFixture]
    public class TestReadOnlyStorage : AbstractIOTestCase
    {
        [Test]
        public void CanOpen()
        {
            var path = NewFileWithBytes();
            var bin = toTest.Open(new BinConfiguration(path, true, 0, true));
            Assert.NotNull(bin);
        }
        [Test]
        public void CannotDelete()
        {
            var path = NewFileWithBytes();
            Assert.Throws<IOException>(()=>toTest.Delete(path));
        }
        [Test]
        public void CannotRename()
        {
            var path = NewFileWithBytes();
            Assert.Throws<IOException>(() => toTest.Rename(path,"newPath"));
        }
        [Test]
        public void Exists()
        {
            var path = NewFileWithBytes();
            var exists = toTest.Exists(path);
            Assert.IsTrue(exists);
        }
        [Test]
        public void DoesNotExists()
        {
            var path = NewFileWithBytes();
            File.Delete(path);
            var exists = toTest.Exists(path);
            Assert.IsFalse(exists);
        }
    }
}