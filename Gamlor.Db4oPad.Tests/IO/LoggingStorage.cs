using System;
using System.Diagnostics;
using Db4objects.Db4o.IO;
using Sharpen.Lang;

namespace Gamlor.Db4oExt.Tests.IO
{
    public class LoggingStorage : StorageDecorator
    {
        public LoggingStorage(IStorage storage) : base(storage)
        {
        }

        public override IBin Open(BinConfiguration config)
        {
            return new LoggingBin(base.Open(config));
        }

        class LoggingBin : BinDecorator
        {
            private long usedInReads = 0;
            private long usedInWrites = 0;
            private long usedInFlushs = 0;
            private long usedInOthers = 0;
            private readonly Stopwatch watch = new Stopwatch();
            public LoggingBin(IBin bin) : base(bin)
            {
            }

            public override void Close()
            {
                watch.Restart();
                base.Close();
                Console.Out.WriteLine("Closing Time used: {0}",watch.ElapsedMilliseconds);
                Console.Out.WriteLine("usedInReads: {0}", usedInReads);
                Console.Out.WriteLine("usedInWrites: {0}", usedInWrites);
                Console.Out.WriteLine("usedInFlushs: {0}", usedInFlushs);
                Console.Out.WriteLine("usedInOthers: {0}", usedInOthers);
            }

            private void Log(ref long counter,Action operation)
            {
                watch.Restart();
                operation();
                counter += watch.ElapsedMilliseconds;
            }
            private T Log<T>(ref long counter, Func<T> operation)
            {
                watch.Restart();
                var result =operation();
                counter += watch.ElapsedMilliseconds;
                return result;
            }

            public override long Length()
            {
                return Log<long>(ref usedInOthers, base.Length);
            }

            public override int Read(long position, byte[] bytes, int bytesToRead)
            {
                return Log(ref usedInReads, ()=>base.Read(position, bytes, bytesToRead));
            }

            public override int SyncRead(long position, byte[] bytes, int bytesToRead)
            {
                throw new NotImplementedException();
            }

            public override void Sync()
            {
                Log(ref usedInFlushs, base.Sync);
            }

            public override void Write(long position, byte[] bytes, int bytesToWrite)
            {
                Log(ref usedInWrites,()=> base.Write(position, bytes, bytesToWrite));
            }

            public override void Sync(IRunnable runnable)
            {
                Log(ref usedInFlushs, ()=>base.Sync(runnable));
            }
        }
    }
}