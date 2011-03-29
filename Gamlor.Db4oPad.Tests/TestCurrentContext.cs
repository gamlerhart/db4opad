using System;
using System.Linq;
using System.Threading.Tasks;
using Db4objects.Db4o;
using Gamlor.Db4oPad.Tests.TestTypes;
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
        [Test]
        public void DisposeWhenClosesContext()
        {
            var db = MemoryDBForTests.NewDB();
            CurrentContext.NewContext(NewContext(db));
            CurrentContext.CloseContext();
            Assert.IsTrue(db.Ext().IsClosed());
        }
        [Test]
        public void DirectQuery()
        {
            var db = MemoryDBForTests.NewDB();
            db.Store(new ClassWithFields());
            CurrentContext.NewContext(NewContext(db));
            var query = CurrentContext.Query<ClassWithFields>();
            Assert.AreNotEqual(0,query.Count());
            CurrentContext.CloseContext();

        }
        
        private DatabaseContext NewContext()
        {
            return NewContext(MemoryDBForTests.NewDB());
        }

        private DatabaseContext NewContext(IObjectContainer db)
        {
            return DatabaseContext.Create(db, TestUtils.NewName(),TestUtils.TestTypeResolver());
        }
    }

    static class StaticQueryConetext
    {
        public static IQueryable<ClassWithFields> ClassWithFields
        {
            get { return CurrentContext.Query<ClassWithFields>(); }
        }
    }
}