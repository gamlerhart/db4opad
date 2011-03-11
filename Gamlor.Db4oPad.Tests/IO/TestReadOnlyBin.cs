using System;
using System.IO;
using System.Linq;
using Db4objects.Db4o.IO;
using Gamlor.Db4oPad.IO;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.IO
{
    [TestFixture]
    public class TestReadOnlyBin : AbstractIOTestCase
    {
        private IBin bin;
        private string path;

        internal override void AdditionalSetup(ReadOnlyStorage readOnlyStorage)
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
        public void CanVirtualWrite()
        {
            var written = new byte[]{1,1,1,1};
            bin.Write(0, written, written.Length);
            var read = new byte[5];
            bin.SyncRead(0, read, 4);
            Assert.IsTrue(written.SequenceEqual(read));
            AssertNoWritesDone();
        }

        private void AssertNoWritesDone()
        {
            bin.Close();
            var fileBytes = File.ReadAllBytes(path);
            Assert.IsTrue(existingData.SequenceEqual(fileBytes));
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
        public void SyncIsNoOperations()
        {
            bin.Sync();
            bin.Sync(null);
        }

        private IBin OpenBin()
        {
            this.path = NewFileWithBytes();
            return toTest.Open(new BinConfiguration(path, true, 0, true));
        }
    }
}