using System;
using System.IO;
using Db4objects.Db4o.IO;
using Gamlor.Db4oExt.IO;
using NUnit.Framework;

namespace Gamlor.Db4oExt.Tests.IO
{
    public abstract class AbstractIOTestCase
    {
        internal const int MegaByte = 1024 * 1024;
        protected static readonly byte[] existingData = InitializeMegaByte();

        private static byte[] InitializeMegaByte()
        {
            var bytes = new byte[MegaByte];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 42;
            }
            return bytes;
        }

        internal IStorage toTest;

        [SetUp]
        public void Setup()
        {
            this.toTest = CreateStorage();
            AdditionalSetup(toTest);
        }

        protected virtual IStorage CreateStorage()
        {
            return AggressiveCacheStorage.RegularStorage();
        }

        internal virtual void AdditionalSetup(IStorage aggressiveCacheStorage)
        {
        }

        protected static string NewFileWithBytes()
        {
            var file = Path.GetTempFileName();
            File.WriteAllBytes(file, existingData);
            return file;
        }
    }
}