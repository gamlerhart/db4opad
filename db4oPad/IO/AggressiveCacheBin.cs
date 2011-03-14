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
            InitializePages(file.Length, file);
        }

        public long Length()
        {
            return file.Length;
        }

        public int Read(long position, byte[] bytes, int bytesToRead)
        {
            return ReadFromPage(position,bytes, bytesToRead,b=>b.Read);
        }


        public void Write(long position, byte[] bytes, int bytesToWrite)
        {
            file.Seek(position, SeekOrigin.Begin);
            file.Write(bytes, 0,bytesToWrite);
        }

        public void Sync()
        {
            // Note that use a write through stream
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
            file.Dispose();
        }


        private delegate int ActionOnPage(int startPositionOnPage,
                                          byte[] data,
                                          int positionOnArray,
                                          int bytesToProcessOnPage);

        private int ReadFromPage(long position,
            byte[] bytes,
            int bytesToRead,
            Func<Page, ActionOnPage> action)
        {
            var currentPage = (int)(position / Page.PageSize);
            var startPositionOnPage = (int)(position % Page.PageSize);
            var bytesConsumed = 0;
            while (bytesConsumed < bytesToRead)
            {
                var readFromThisPage = PageAt(currentPage);
                var bytesToReadFromPage = Math.Min(Page.PageSize - startPositionOnPage, bytesToRead - bytesConsumed);
                var bytesReadFromPage = action(readFromThisPage)(startPositionOnPage, bytes, bytesConsumed,
                                                                 bytesToReadFromPage);
                bytesConsumed += bytesReadFromPage;

                if (0 == bytesConsumed || bytesReadFromPage < bytesToReadFromPage)
                {
                    return bytesConsumed;
                }

                currentPage++;
                startPositionOnPage = 0;
            }
            return bytesConsumed;
        }

        private Page PageAt(int currentPage)
        {
            EnsureEnoughtPagesFor(currentPage);
            return pages[currentPage];
        }

        private void InitializePages(long length, FileStream stream)
        {
            this.pages = new List<Page>(AmountOfPages(length));
            EnsureEnoughtPagesFor(AmountOfPages(stream.Length));
        }

        private void EnsureEnoughtPagesFor(int needAtLeast)
        {
            if (pages.Count <= needAtLeast)
            {
                var amountToCreate = (needAtLeast - pages.Count) + 1;
                for (int i = 0; i < amountToCreate; i++)
                {
                    pages.Add(new Page(pages.Count, file));

                }
            }
        }

        private static int AmountOfPages(long length)
        {
            return (int)(length / Page.PageSize) + 1;
        }

    }
}