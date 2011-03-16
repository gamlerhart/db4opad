using System;
using System.IO;
using System.Threading.Tasks;

namespace Gamlor.Db4oExt.IO
{
    public abstract class IOCoordination : IDisposable
    {
        private readonly FileStream file;
        private Task writeBackTask;
        private readonly object fileStreamLock = new object();

        protected IOCoordination(FileStream file)
        {
            this.file = file;
            writeBackTask = new Task(()=> { });
            writeBackTask.Start();
        }

        public static IOCoordination Create(FileStream file)
        {
            return new RegularImplementation(file);
        }

        public static IOCoordination NowWriteBack(FileStream file)
        {
            return new IOCoordinationNoWriteBack(file);
        }

        public abstract void Write(long position, byte[] bytes, int bytesToWrite);
        public abstract void Flush();


        public int Read(long position, byte[] bytes, int bytesToWrite)
        {
            lock (fileStreamLock)
            {
                file.Seek(position, SeekOrigin.Begin);
                return file.Read(bytes, 0, bytesToWrite);
            }
        }


        private void WriteTask(long position, byte[] bytes, int bytesToWrite)
        {
            lock (fileStreamLock)
            {
                file.Seek(position, SeekOrigin.Begin);
                file.Write(bytes, 0, bytesToWrite);
            }
        }


        private void FlushTask()
        {
            lock (fileStreamLock)
            {
                file.Flush();
            }
        }

        public void Dispose()
        {
            writeBackTask.Wait();
            lock (fileStreamLock)
            {
                file.Dispose();
            }
        }

        class IOCoordinationNoWriteBack : IOCoordination
        {
            public IOCoordinationNoWriteBack(FileStream file) : base(file)
            {
            }

            public override void Write(long position, byte[] bytes, int bytesToWrite)
            {
                // no operation
            }

            public override void Flush()
            {
                // no operation
            }
        }
        class RegularImplementation : IOCoordination
        {
            public RegularImplementation(FileStream file) : base(file)
            {
            }
            public override void Write(long position, byte[] bytes, int bytesToWrite)
            {
                writeBackTask = writeBackTask.ContinueWith(t => WriteTask(position, bytes,
                    bytesToWrite));
            }
            public override void Flush()
            {
                writeBackTask = writeBackTask.ContinueWith(t => FlushTask());
            }
        }
    }
}