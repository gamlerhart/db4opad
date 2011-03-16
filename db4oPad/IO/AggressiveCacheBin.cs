using System;
using System.Collections.Generic;
using System.Diagnostics;
using Db4objects.Db4o.IO;
using Sharpen.Lang;

namespace Gamlor.Db4oExt.IO
{
    class AggressiveCacheBin : IBin
    {
        private List<Page> pages;
        private long length;
        private IOCoordination ioCoordination;

        public AggressiveCacheBin(IOCoordination file, long fileLenght)
        {
            this.length = fileLenght;
            this.ioCoordination = file;
            InitializePages(fileLenght);
        }

        public long Length()
        {
            return length;
        }

        public int Read(long position, byte[] bytes, int bytesToRead)
        {
            return DoActionOnPage(position,bytes, bytesToRead,b=>b.Read);
        }


        public void Write(long position, byte[] bytes, int bytesToWrite)
        {
            var sw = Stopwatch.StartNew();
            var amoutWritten = DoActionOnPage(position, bytes, bytesToWrite, b => b.Write);
            var lastByte = position + amoutWritten;
            if(lastByte > length)
            {
                length = lastByte;
            }
            ioCoordination.Write(position, bytes, bytesToWrite);
        }

        public void Sync()
        {
            // Note that use a write through stream
            ioCoordination.Flush();
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
            ioCoordination.Dispose();
        }


        private delegate int ActionOnPage(int startPositionOnPage,
                                          byte[] data,
                                          int positionOnArray,
                                          int bytesToProcessOnPage);

        private int DoActionOnPage(long position,
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
            EnsureEnoughtPagesFor(currentPage,i=>Page.NewEmptyPage(i,ioCoordination));
            return pages[currentPage];
        }

        private void InitializePages(long length)
        {
            this.pages = new List<Page>(AmountOfPages(length));
            EnsureEnoughtPagesFor(AmountOfPages(length) - 1,i=>Page.ExistingPage(i,ioCoordination));
        }

        private void EnsureEnoughtPagesFor(int pageIndex,Func<int,Page> constructor)
        {
            if (pages.Count <= pageIndex)
            {
                var amountToCreate = (pageIndex - pages.Count)+1;
                for (int i = 0; i < amountToCreate; i++)
                {
                    pages.Add(constructor(pages.Count));

                }
            }
        }

        private static int AmountOfPages(long length)
        {
            return (int)(length / Page.PageSize) + 1;
        }

    }
}