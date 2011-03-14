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
            this.pages = CreatePages(file.Length,file);
        }

        public long Length()
        {
            return file.Length;
        }

        public int Read(long position, byte[] bytes, int bytesToRead)
        {
            return ReadFromPage(position,bytes, bytesToRead);
//            file.Seek(position, SeekOrigin.Begin);
//            return file.Read(bytes, 0,bytesToRead);
        }

        private int ReadFromPage(long position,byte[] bytes, int bytesToRead)
        {
            var currentPage = (int)(position/Page.PageSize);
            var startPositionOnPage = (int) (position%Page.PageSize);
            var bytesConsumed = 0;
            while (bytesConsumed < bytesToRead)
            {
                var readFromThisPage = PageAt(currentPage);
                var bytesToReadFromPage = Math.Min(Page.PageSize - startPositionOnPage, bytesToRead - bytesConsumed);
                var bytesReadFromPage =readFromThisPage.Read(startPositionOnPage, bytes, bytesConsumed, bytesToReadFromPage);
                bytesConsumed += bytesReadFromPage;

                if (0 == bytesConsumed ||  bytesReadFromPage < bytesToReadFromPage)
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
            if(pages.Count<=currentPage)
            {
                var difference = (currentPage - pages.Count)+1;
                for (int i = 0; i < difference;i++ )
                {
                    pages.Add(new Page(pages.Count, file));

                }
            }
            return pages[currentPage];
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

        private static List<Page> CreatePages(long length, FileStream stream)
        {
            var list = new List<Page>(AmountOfPages(length));
            for (int i = 0; i < AmountOfPages(length); i++)
            {
                list.Add(new Page(i,stream));
            }
            return list;
        }

        private static int AmountOfPages(long length)
        {
            return (int)(length / Page.PageSize) + 1;
        }

    }


    class Page
    {
        internal const int PageSize = 512;
        private readonly int pageNumber;
        private readonly FileStream theStream;

        public Page(int pageNumber,FileStream theStream)
        {
            this.pageNumber = pageNumber;
            this.theStream = theStream;
        }


        public int Read(int startPositionOnPage,
            byte[] bytes,
            int writePositionOnArray, 
            int bytesToReadFromPage)
        {
            var temp = new byte[Page.PageSize];
            theStream.Seek(pageNumber*PageSize, SeekOrigin.Begin);
            var bytesRead = theStream.Read(temp, 0, PageSize);
            var amoutToCopy = Math.Min(bytesRead, bytesToReadFromPage);
            Array.Copy(temp, startPositionOnPage, bytes, writePositionOnArray, amoutToCopy);
            return amoutToCopy;
        }
    }

    enum PageState
    {
        NotLoaded
    }
}