using System;
using System.Collections.Generic;
using System.IO;
using Db4objects.Db4o.IO;
using Sharpen.Lang;

namespace Gamlor.Db4oExt.IO
{
    class AggressiveCacheBin : IBin
    {
        private readonly FileStream file;
        private List<Page> pages;

        public AggressiveCacheBin(FileStream file)
        {
            this.file = file;
            this.pages = CreatePages(file.Length);
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

        private List<Page> CreatePages(long length)
        {
            var list = new List<Page>(AmountOfPages(length));
            for (int i = 0; i < AmountOfPages(length); i++)
            {
                list.Add(new Page());
            }
            return list;
        }

        private int AmountOfPages(long length)
        {
            return (int)(length / Page.PageSize) + 1;
        }

    }


    class Page
    {
        internal const int PageSize = 512;
        private readonly byte[] data = new byte[PageSize];

        
        
    }
}