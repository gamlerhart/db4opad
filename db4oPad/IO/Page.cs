using System;
using System.IO;

namespace Gamlor.Db4oExt.IO
{
    class Page
    {
        internal const int PageSize = 512;
        private readonly int pageNumber;
        private readonly FileStream theStream;

        private ReadStateAction currentState; 
        private readonly object sync = new object();
        private byte[] pageData;
        private int pageLength;


        private delegate int ReadStateAction(int startPositionOnPage,
                                             byte[] bytes,
                                             int writePositionOnArray,
                                             int bytesToReadFromPage);



        public Page(int pageNumber,FileStream theStream)
        {
            this.pageNumber = pageNumber;
            this.theStream = theStream;
            this.currentState = EmptyState;
        }


        public int Read(int startPositionOnPage,
                        byte[] bytes,
                        int writePositionOnArray, 
                        int bytesToReadFromPage)
        {
            lock (sync)
            {
                return currentState(startPositionOnPage, bytes, writePositionOnArray, bytesToReadFromPage);
            }
        }

        int EmptyState(int startPositionOnPage,
                        byte[] bytes,
                        int writePositionOnArray, 
                        int bytesToReadFromPage)
        {
            pageData = new byte[PageSize];
            theStream.Seek(pageNumber*PageSize, SeekOrigin.Begin);
            pageLength = theStream.Read(pageData, 0, PageSize);
            var amoutToCopy = Math.Min(pageLength, bytesToReadFromPage);
            Array.Copy(pageData, startPositionOnPage, bytes, writePositionOnArray, amoutToCopy);
            //SwitchToState(CachePageLoadedState);
            return amoutToCopy;
        }
        int CachePageLoadedState(int startPositionOnPage,
                        byte[] bytes,
                        int writePositionOnArray,
                        int bytesToReadFromPage)
        {
            var amoutToCopy = Math.Min(pageLength, bytesToReadFromPage);
            Array.Copy(pageData, startPositionOnPage, bytes, writePositionOnArray, amoutToCopy);
            return amoutToCopy;
        }

        private void SwitchToState(ReadStateAction theNewState)
        {
            currentState = theNewState;
        }
    }
}