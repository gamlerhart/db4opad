using System;
using System.IO;
using Db4objects.Db4o.IO;

namespace Gamlor.Db4oExt.IO
{
    class AggressiveCacheStorage : IStorage
    {

        private Func<string,IOCoordination> coordinatorFactory;

        private AggressiveCacheStorage(Func<string, IOCoordination> coordinatorFactory)
        {
            this.coordinatorFactory = coordinatorFactory;
        }


        public static IStorage RegularStorage()
        {
            return new AggressiveCacheStorage(ReadWriteStream);
        }
        public static IStorage NoWriteBack()
        {
            return new AggressiveCacheStorage(ReadOnly);
        }

        public IBin Open(BinConfiguration config)
        {
            var theBin = new AggressiveCacheBin(coordinatorFactory(config.Uri()), new FileInfo(config.Uri()).Length);
            FillUpBytes(theBin,config.InitialLength());
            return theBin;
        }

        public bool Exists(string uri)
        {
            return File.Exists(uri) && new FileInfo(uri).Length>0;
        }

        public void Delete(string uri)
        {
            throw new IOException("Cannot delete files with the read only storage");
        }

        public void Rename(string oldUri, string newUri)
        {
            throw new IOException("Cannot rename files with the read only storage");
        }

        private static IOCoordination ReadWriteStream(string path)
        {
            return IOCoordination.Create(NewStream(path,FileAccess.ReadWrite));
        }

        private static IOCoordination ReadOnly(string path)
        {
            return IOCoordination.NowWriteBack(NewStream(path, FileAccess.Read));
        }

        private static FileStream NewStream(string path, FileAccess access)
        {
            return new FileStream(path,
                                  FileMode.OpenOrCreate,
                                  access,
                                  FileShare.None,
                                  128,
                                  FileOptions.WriteThrough);
        }

        private void FillUpBytes(AggressiveCacheBin theBin, long initialLength)
        {
            if (theBin.Length() < initialLength)
            {
                var bytes = new byte[initialLength - theBin.Length()];
                theBin.Write(theBin.Length(), bytes, bytes.Length);
            }
        }

    }
}