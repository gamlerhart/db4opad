using System;

namespace Gamlor.Db4oPad.Utils
{
    public sealed class Disposer : IDisposable
    {
        private event Action DisposeEvent;
        private volatile bool disposed;

        public void Dispose()
        {
            // Yes, the check and setting to disposed is a race-condition
            // However this is only to catch obvious double disposes
            // and therefore it doesn't matter that much
            CheckNotDisposed();
            disposed = true;
            var toDispose = DisposeEvent;
            if (null != toDispose)
            {
                toDispose();
            }
            DisposeEvent = null;
        }


        public void Add(IDisposable toDispose)
        {
            CheckNotDisposed();
            DisposeEvent += toDispose.Dispose;
        }

        public void AddIfDisposable(object toDispose)
        {
            if (toDispose is IDisposable)
            {
                Add((IDisposable)toDispose);
            }
        }

        public void AddAsDisposable(Action action)
        {
            Add(AsDisposable(action));
        }

        public void CheckNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("This object is already disposed");
            }
        }

        public static IDisposable AsDisposable(Action disposeAction)
        {
            if (null == disposeAction)
            {
                throw new ArgumentNullException("disposeAction");
            }
            return new ClosureDisposable(disposeAction);
        }
    }
    public static class DisposerExtensions
    {
        public static void Event<T>(this Disposer disposer,
            T eventHandler,
            Action<T> registration,
            Action<T> deRegistration)
        {
            registration(eventHandler);
            var disposable = Disposer.AsDisposable(() => deRegistration(eventHandler));
            disposer.Add(disposable);

        }
    }

    internal class ClosureDisposable : IDisposable
    {
        private Action toRun;

        public ClosureDisposable(Action toRun)
        {
            this.toRun = toRun;
        }


        public void Dispose()
        {
            if (null == toRun)
            {
                throw new ObjectDisposedException("Cannot dispose twice");
            }
            toRun();
            toRun = null;
        }

    }

}