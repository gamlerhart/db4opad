using System;
using System.IO;

namespace Gamlor.Db4oExt.IO
{
    class Page
    {
        internal const int PageSize = 512;
        private readonly int pageNumber;
        private readonly FileStream theStream;

        private PageState currentState; 
        private readonly object sync = new object();
        private byte[] pageData;
        private int pageLength;


        public Page(int pageNumber,FileStream theStream)
        {
            this.pageNumber = pageNumber;
            this.theStream = theStream;
            this.currentState = PageState.EmptyState;
        }


        public int Read(int startPositionOnPage,
                        byte[] bytes,
                        int writePositionOnArray, 
                        int bytesToReadFromPage)
        {
            lock (sync)
            {
                return ApplyAndSwitchState(
                    currentState.Read,
                    new RequestParameters(startPositionOnPage,
                    bytes, 
                    writePositionOnArray, 
                    bytesToReadFromPage));
            }
        }

        public int Write(int startPositionOnPage,
                        byte[] bytes,
                        int writePositionOnArray,
                        int bytesToReadFromPage)
        {
            lock (sync)
            {
                return ApplyAndSwitchState(
                    currentState.Write,
                    new RequestParameters(startPositionOnPage,
                    bytes,
                    writePositionOnArray,
                    bytesToReadFromPage));
            }
        }

        private int ApplyAndSwitchState(Func<Page,RequestParameters,Tuple<int,PageState>> action,
            RequestParameters parameters)
        {
            var result = action(this, parameters);
            SwitchToState(result.Item2);
            return result.Item1;
        }


        private void SwitchToState(PageState theNewState)
        {
            currentState = theNewState;
        }

        private struct RequestParameters
        {
            public RequestParameters(int startPositionOnPage,
                byte[] bytes, 
                int writePositionOnArray,
                int bytesToReadFromPage) : this()
            {
                StartPositionOnPage = startPositionOnPage;
                Bytes = bytes;
                WritePositionOnArray = writePositionOnArray;
                BytesToReadFromPage = bytesToReadFromPage;
            }

            public byte[] Bytes { get; private set; }

            public int StartPositionOnPage { get; private set; }

            public int WritePositionOnArray { get; private set; }

            public int BytesToReadFromPage { get; private set; }
        }

        abstract class PageState
        {
            public abstract Tuple<int,PageState> Read(Page thePage,
                RequestParameters parameters);

            public abstract Tuple<int, PageState> Write(Page thePage,
                RequestParameters parameters);

            public static readonly PageState EmptyState = new EmptyStateImpl();
            static readonly PageState CachePageLoadedState = new CachePageLoadedStateImpl();



            protected Tuple<int, PageState> WriteOnPage(Page thePage, RequestParameters parameters)
            {
                Array.Copy(parameters.Bytes,
                           parameters.WritePositionOnArray,
                           thePage.pageData,
                           parameters.StartPositionOnPage, parameters.BytesToReadFromPage);
                thePage.pageLength = Math.Max(parameters.StartPositionOnPage + parameters.BytesToReadFromPage,
                    thePage.pageLength);
                return Tuple.Create(parameters.BytesToReadFromPage, CachePageLoadedState);
            }

            private class EmptyStateImpl : PageState
            {
                public override Tuple<int, PageState> Read(Page thePage,
                    RequestParameters parameters)
                {
                    ReadDataIntoPage(thePage);
                    var amoutToCopy = Math.Min(thePage.pageLength, parameters.BytesToReadFromPage);
                    Array.Copy(thePage.pageData, parameters.StartPositionOnPage, parameters.Bytes, parameters.WritePositionOnArray, amoutToCopy);
                    return Tuple.Create(amoutToCopy, CachePageLoadedState);
                }

                public override Tuple<int, PageState> Write(Page thePage,
                   RequestParameters parameters)
                {
                    ReadDataIntoPage(thePage);
                    return WriteOnPage(thePage, parameters);
                }

                private void ReadDataIntoPage(Page thePage)
                {
                    thePage.pageData = new byte[PageSize];
                    thePage.theStream.Seek(thePage.pageNumber * PageSize, SeekOrigin.Begin);
                    thePage.pageLength = thePage.theStream.Read(thePage.pageData, 0, PageSize);
                }

            }

            private class CachePageLoadedStateImpl : PageState
            {
                public override Tuple<int, PageState> Read(Page thePage,
                    RequestParameters parameters)
                {
                    var amoutToCopy = Math.Min(thePage.pageLength, parameters.BytesToReadFromPage);
                    Array.Copy(thePage.pageData, parameters.StartPositionOnPage, parameters.Bytes, parameters.WritePositionOnArray, amoutToCopy);
                    return Tuple.Create(amoutToCopy, CachePageLoadedState);
                }
                public override Tuple<int, PageState> Write(Page thePage,
                   RequestParameters parameters)
                {
                    return WriteOnPage(thePage, parameters);
                }
            }
        }


    }
}