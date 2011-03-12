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
        private const int AmountOfRandomReadsWrites =5*KiloByte;
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
        public void CanWrite()
        {
            var written = new byte[]{1,1,1,1};
            bin.Write(0, written, written.Length);
            var read = new byte[4];
            bin.Read(0, read, 4);
            Assert.IsTrue(written.SequenceEqual(read));
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
        [Test]
        public void FileIsExpandedAfterClosing()
        {
            var oldLenght = bin.Length();
            var written = new byte[] { 1, 1, 1, 1 };
            bin.Write(oldLenght - 2, written, written.Length);
            bin.Close();
            var bytesOnDisk = File.ReadAllBytes(path);
            Assert.AreEqual(oldLenght+2,bytesOnDisk.Length);
            Assert.IsTrue(written.SequenceEqual(bytesOnDisk.Skip(bytesOnDisk.Length-4)));
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
        public void RandomWritesReads()
        {
            for(var i=0;i<AmountOfRandomReadsWrites;i++)
            {
                var bytesToWrite = new byte[i];
                rnd.NextBytes(bytesToWrite);
                var position = rnd.Next((int)bin.Length());
                bin.Write(position,bytesToWrite,bytesToWrite.Length);

                var bytesRead = new byte[i];
                var readedAmount = bin.Read(position, bytesRead, bytesToWrite.Length);
                Assert.AreEqual(readedAmount, bytesToWrite.Length);
                Assert.IsTrue(bytesToWrite.SequenceEqual(bytesRead));

            }
        }

        [Test]
        public void ReturnsBytesRead()
        {
            var readedAmount = bin.Read(bin.Length()-2, new byte[4], 4);
            Assert.AreEqual(2,readedAmount);
        }
        [Test]
        public void OnlyReadSection()
        {
            var bytes = new byte[] {0, 0, 0, 1, 2, 3};
            bin.Read(16, bytes, 3);
            Assert.IsTrue(bytes.Take(3).SequenceEqual(new byte[]{42,42,42}));
            Assert.IsTrue(bytes.Skip(3).SequenceEqual(new byte[]{1,2,3}));
        }
        [Test]
        public void OnlyWriteSection()
        {
            var write = new byte[] { 1, 2,3, 4, 5, 6 };
            bin.Write(16, write, 3);

            var read = new byte[write.Length];
            bin.Read(16, read, 6);
            Assert.IsTrue(read.Take(3).SequenceEqual(new byte[] {1, 2, 3 }));
            Assert.IsTrue(read.Skip(3).SequenceEqual(new byte[] { 42, 42, 42 }));
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


        [Test]
        public void OpenWithGivenSize()
        {
            var theBin = (AggressiveCacheBin)toTest.Open(new BinConfiguration(Path.GetRandomFileName(), true,KiloByte, true));
            Assert.AreEqual(KiloByte, theBin.Length());
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