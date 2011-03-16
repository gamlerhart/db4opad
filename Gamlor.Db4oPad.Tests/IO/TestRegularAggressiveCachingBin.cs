using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Gamlor.Db4oExt.Tests.IO
{
    [TestFixture]
    public class TestRegularAggressiveCachingBin : AggressiveCacheBinTestBase
    {

        [Test]
        public void FileIsExpandedAfterClosing()
        {
            var oldLenght = bin.Length();
            var written = new byte[] { 1, 1, 1, 1 };
            bin.Write(oldLenght - 2, written, written.Length);
            bin.Close();
            var bytesOnDisk = File.ReadAllBytes(path);
            Assert.AreEqual(oldLenght + 2, bytesOnDisk.Length);
            Assert.IsTrue(written.SequenceEqual(bytesOnDisk.Skip(bytesOnDisk.Length - 4)));
        }
        [Test]
        public void IsWrittenAfterClosing()
        {
            var written = new byte[] { 1, 1, 1, 1 };
            bin.Write(0, written, written.Length);
            bin.Close();
            var bytesOnDisk = File.ReadAllBytes(path).Take(4);
            Assert.IsTrue(written.SequenceEqual(bytesOnDisk));
        }
    }
}