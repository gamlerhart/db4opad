using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class TestCurrentContext
    {

        [Test]
        public void CanSetContext()
        {
            var context = NewContext();
            CurrentContext.NewContext(context);
            Assert.AreEqual(context, CurrentContext.GetCurrentContext());
            CurrentContext.CloseContext();
        }
        [Test]
        public void IsThreadLocal()
        {
            var task = new Task(() => CurrentContext.NewContext(NewContext()));
            task.Start();
            task.Wait();
            Assert.Throws(typeof(InvalidOperationException), () => CurrentContext.GetCurrentContext());
            CurrentContext.CloseContext();
        }
        [Test]
        public void ClosesContext()
        {
            CurrentContext.NewContext(NewContext());
            CurrentContext.CloseContext();
            Assert.Throws(typeof(InvalidOperationException), () => CurrentContext.GetCurrentContext());

        }

        private DatabaseContext NewContext()
        {
            var db = MemoryDBForTests.NewDB();
            return DatabaseContext.Create(db, new AssemblyName("temp")
                                                  {
                                                      CodeBase = Path.GetTempFileName()
                                                  });
        }
    }
}