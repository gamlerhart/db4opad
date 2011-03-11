using System.Collections.Generic;
using System.IO;
using Db4objects.Db4o.IO;

namespace Gamlor.Db4oPad.IO
{
    class ReadOnlyStorage : IStorage
    {
        public IBin Open(BinConfiguration config)
        {
            return new ReadOnlyBin(new FileStream(config.Uri(),FileMode.OpenOrCreate,FileAccess.Read));
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