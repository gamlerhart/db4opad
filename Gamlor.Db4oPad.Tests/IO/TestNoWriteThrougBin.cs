using System.IO;
using System.Linq;
using Db4objects.Db4o.IO;
using Gamlor.Db4oExt.IO;
using NUnit.Framework;

namespace Gamlor.Db4oExt.Tests.IO
{
    [TestFixture]
    public class TestNoWriteThrougBin  : AggressiveCacheBinTestBase
    {
        [Test]
        public void FileIsExpandedAfterClosing()
        {
            var oldLenght = bin.Length();
            var written = new byte[] { 1, 1, 1, 1 };
            bin.Write(oldLenght - 2, written, written.Length);
            bin.Close();
            var bytesOnDisk = File.ReadAllBytes(path);
            Assert.AreEqual(oldLenght, bytesOnDisk.Length);
            Assert.IsTrue(bytesOnDisk.All(b=>b==42));
        }
        [Test]
        public void IsWrittenAfterClosing()
        {
            var written = new byte[] { 1, 1, 1, 1 };
            bin.Write(0, written, written.Length);
            bin.Close();
            var bytesOnDisk = File.ReadAllBytes(path);
            Assert.IsTrue(bytesOnDisk.All(b => b == 42));
        }


        protected override IStorage CreateStorage()
        {
            return AggressiveCacheStorage.NoWriteBack();
        }
    }
}