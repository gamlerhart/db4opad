using System;
using System.IO;
using Db4objects.Db4o.IO;
using Sharpen.Lang;

namespace Gamlor.Db4oExt.IO
{
    class AggressiveCacheBin : IBin
    {
        private readonly FileStream file;

        public AggressiveCacheBin(FileStream file)
        {
            this.file = file;
        }

        public long Length()
        {
            return file.Length;
        }

        public int Read(long position, byte[] bytes, int bytesToRead)
        {
            file.Seek(position, SeekOrigin.Begin);
            return file.Read(bytes, 0,bytesToRead);
        }

        public void Write(long position, byte[] bytes, int bytesToWrite)
        {
            file.Seek(position, SeekOrigin.Begin);
            file.Write(bytes, 0,bytesToWrite);
        }

        public void Sync()
        {
            // Not that we expect a write through stream
            file.Flush();
        }

        public void Sync(IRunnable runnable)
        {
            Sync();
            runnable.Run();
            Sync();
        }

        public int SyncRead(long position, byte[] bytes, int bytesToRead)
        {
            throw new NotSupportedException("wtf");
        }

        public void Close()
        {
            this.file.Dispose();
        }

    }
}