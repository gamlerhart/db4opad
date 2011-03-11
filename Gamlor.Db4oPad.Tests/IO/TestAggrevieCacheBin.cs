using System;
using System.IO;
using System.Linq;
using Db4objects.Db4o.IO;
using Gamlor.Db4oExt.IO;
using NUnit.Framework;

namespace Gamlor.Db4oExt.Tests.IO
{
    [TestFixture]
    public class TestAggrevieCacheBin : AbstractIOTestCase
    {
        private readonly Random rnd = new Random();
        private const int KiloByte = 1024;
        private AggressiveCacheBin bin;
        private string path;

        internal override void AdditionalSetup(AggressiveCacheStorage aggressiveCacheStorage)
        {
            this.bin = OpenBin();
        }

        [Test]
        public void CanRead()
        {
            var bytes = new byte[5];
            bin.Read(0, bytes, 5);
            Assert.IsTrue(existingData.Take(5).SequenceEqual(bytes));
        }
        [Test]
        public void CanReadSync()
        {
            var bytes = new byte[5];
            bin.SyncRead(0, bytes, 5);
            Assert.IsTrue(existingData.Take(5).SequenceEqual(bytes));
        }


        [Test]
        public void CanWrite()
        {
            var written = new byte[]{1,1,1,1};
            bin.Write(0, written, written.Length);
            var read = new byte[4];
            bin.Read(0, read, 4);
            Assert.IsTrue(written.SequenceEqual(read));
        }
        [Test]
        public void CanWriteInTheMiddle()
        {
            var written = new byte[] { 1, 1, 1, 1 };
            bin.Write(4, written, written.Length);
            var read = new byte[4];
            bin.Read(4, read, 4);
            Assert.IsTrue(written.SequenceEqual(read));
        }
        [Test]
        public void CanWriteLargeSections()
        {
            var written = RandomBytes(50*KiloByte);
            bin.Write(4, written, written.Length);
            var read = RandomBytes(50 * KiloByte);
            bin.Read(4, read, 50 * KiloByte);
            Assert.IsTrue(written.SequenceEqual(read));
        }
        [Test]
        public void CanWriteOverFileEnd()
        {
            var written = new byte[] { 1, 1, 1, 1 };
            bin.Write(bin.Length()-1, written, written.Length);
            var read = new byte[written.Length];
            bin.Read(bin.Length()-written.Length, read, written.Length);
            Assert.IsTrue(written.SequenceEqual(read));
        }

        [Test]
        public void CanClose()
        {
            bin.Close();
            Assert.Throws<ObjectDisposedException>(() => bin.Read(0, new byte[5], 5));
        }

        [Test]
        public void GetLenght()
        {
            Assert.AreEqual(existingData.Length, bin.Length());
        }

        private AggressiveCacheBin OpenBin()
        {
            this.path = NewFileWithBytes();
            return (AggressiveCacheBin)toTest.Open(new BinConfiguration(path, true, 0, true));
        }

        private byte[] RandomBytes(int amout)
        {
            var bytes = new byte[amout];
            rnd.NextBytes(bytes);
            return bytes;
        }
    }
}