using System;
using System.IO;
using Db4objects.Db4o.IO;
using NUnit.Framework;

namespace Gamlor.Db4oExt.Tests.IO
{
    [TestFixture]
    public class TestAggressiveCacheStorage : AbstractIOTestCase
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
        public void ExistsChecksForZeroBytes()
        {
            var path = Path.GetTempFileName();
            File.WriteAllBytes(path, new byte[0]);
            var exists = toTest.Exists(path);
            Assert.IsFalse(exists);
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