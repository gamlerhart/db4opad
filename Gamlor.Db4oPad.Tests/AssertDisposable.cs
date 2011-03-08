using System;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    public class AssertDisposable : IDisposable
    {
        private bool isDisposed;

        public AssertDisposable()
            : this(false)
        {
        }

        public AssertDisposable(bool disposed)
        {
            isDisposed = disposed;
        }


        public void Dispose()
        {
            AssertNotDisposed();
            isDisposed = true;
        }

        public void AssertNotDisposed()
        {
            Assert.IsFalse(isDisposed);
        }

        public void AssertDisposed()
        {
            Assert.IsTrue(isDisposed);
        }
    }

}