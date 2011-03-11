using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Db4objects.Db4o.IO;
using Gamlor.Db4oPad.Utils;
using Sharpen.Lang;

namespace Gamlor.Db4oPad.IO
{
    class ReadOnlyBin : IBin
    {
        private readonly FileStream file;
        private readonly List<Page> pages = new List<Page>();

        public ReadOnlyBin(FileStream file)
        {
            this.file = file;
        }

        public long Length()
        {
            return file.Length;
        }

        public int Read(long position, byte[] bytes, int bytesToRead)
        {
            var ranges = Range.RangesFor((int)position, bytesToRead);
            int indexOnOutArray = 0;
            foreach (var range in ranges)
            {
                var page = GetPageFor(range);
                indexOnOutArray += page.VirtualRead(range, bytes, indexOnOutArray);
            }
            return indexOnOutArray;
        }

        public void Write(long position, byte[] bytes, int bytesToWrite)
        {
            var ranges = Range.RangesFor((int)position, bytesToWrite);
            int indexOnOutArray = 0;
            foreach (var range in ranges)
            {
                var page = GetPageFor(range);
                indexOnOutArray += page.Write(range, bytes, indexOnOutArray);
            }
        }

        private Page GetPageFor(Range range)
        {
            if(range.PageIndex>=pages.Count)
            {
                var newPage = Page.NewPage(range,file);
                pages.Add(newPage);
                return newPage;
            }
            return pages[range.PageIndex];
        }

        public void Sync()
        {
        }

        public void Sync(IRunnable runnable)
        {
        }

        public int SyncRead(long position, byte[] bytes, int bytesToRead)
        {
            return Read(position, bytes, bytesToRead);
        }

        public void Close()
        {
            file.Close();
        }

        class Page
        {
            public const int PageSize = 512;
            private int bytesUsed = 0;
            private byte[] data = null;
            private readonly FileStream file;

            private Page(FileStream fileStream)
            {
                this.file = fileStream;
            }

            public static Page NewPage(Range range, FileStream file)
            {
                var thePage = new Page(file);
                return thePage;
            }

            private int RealReadRead(byte[] data,Range position)
            {
                return file.Read(data, position.PageIndex * PageSize, PageSize);
            }

            public int Write(Range range, byte[] bytes, int indexOnOutput)
            {
                if(null==data)
                {
                    data = new byte[PageSize];
                    bytesUsed = RealReadRead(data, range);
                }
                for(int i=0;i<range.Amout;i++)
                {
                    bytes[indexOnOutput + i] = data[range.ReadFrom + i];
                    indexOnOutput++;
                }
                return range.Amout;
            }

            public int VirtualRead(Range range, byte[] bytes, int indexOnOutArray)
            {
                if(null==data)
                {
                    return RealReadRead(bytes, range);
                } else
                {
                    return 0;
                }
            }
        }

        class Range
        {
            private readonly int pageIndex;
            private readonly int readFrom;
            private readonly int amout;

            public Range(int range, int readFrom, int amout)
            {
                this.pageIndex = range;
                this.readFrom = readFrom;
                this.amout = amout;
            }

            public static IEnumerable<Range> RangesFor(int position, int toRead)
            {
                return CalculateRange(position, toRead).ToList();
            }

            private static IEnumerable<Range> CalculateRange(int position, int toRead)
            {
                var currentPosition = position;
                var readData = 0;
                while (readData < toRead)
                {
                    var page = currentPosition/Page.PageSize;
                    var readFrom = currentPosition%Page.PageSize;
                    var amout = Math.Min(Page.PageSize - readFrom, (toRead-readData));
                    yield return new Range(page, readFrom, amout);
                    readData += amout;
                    currentPosition += 0;
                }
            }

            public int PageIndex
            {
                get { return pageIndex; }
            }

            public int ReadFrom
            {
                get { return readFrom; }
            }

            public int Amout
            {
                get { return amout; }
            }
        }
    }
}