using System.IO;
using Db4objects.Db4o.IO;

namespace Gamlor.Db4oExt.IO
{
    class AggressiveCacheStorage : IStorage
    {
        public IBin Open(BinConfiguration config)
        {
            var theBin = new AggressiveCacheBin(new FileStream(config.Uri(),
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,32,FileOptions.WriteThrough));
            FillUpBytes(theBin,config.InitialLength());
            return theBin;
        }

        private void FillUpBytes(AggressiveCacheBin theBin, long initialLength)
        {
            if(theBin.Length()<initialLength)
            {
                var bytes = new byte[initialLength - theBin.Length()];
                theBin.Write(theBin.Length(),bytes, bytes.Length);
            }
        }

        public bool Exists(string uri)
        {
            return File.Exists(uri);
        }

        public void Delete(string uri)
        {
            throw new IOException("Cannot delete files with the read only storage");
        }

        public void Rename(string oldUri, string newUri)
        {
            throw new IOException("Cannot rename files with the read only storage");
        }

    }
}