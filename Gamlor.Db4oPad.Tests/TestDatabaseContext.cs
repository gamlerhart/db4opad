using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class TestDatabaseContext : AbstractDatabaseFixture
    {

        protected override void FixtureSetup(IObjectContainer db)
        {
            db.Store(new ClassWithoutFields());
        }

        [Test]
        public void ListsTypes()
        {
            var toTest = NewTestInstance();
            var types = toTest.ListTypes();
            Assert.AreEqual(1,types.Count());
        }

        [Test]
        public void ListsMultipleTypes()
        {
            DB.Store(new ClassWithFields());
            var toTest = NewTestInstance();
            var types = toTest.ListTypes();
            Assert.AreEqual(2, types.Count());
        }
        [Test]
        public void HasRightName()
        {
            var toTest = NewTestInstance();
            var types = toTest.ListTypes();
            Assert.AreEqual("ClassWithoutFields", types.Single().Text);
        }
        [Test]
        public void HasFields()
        {
            DB.Store(new ClassWithFields());
            var toTest = NewTestInstance();
            var types = toTest.ListTypes();
            Assert.IsTrue(types.Where(c=>c.Children.Count>0)
                .Any(c => c.Children.Single().Text == "aField"));
        }
        [Test]
        public void HasWrittenAssembly()
        {
            var name = NewName();
            name.CodeBase = Path.GetTempFileName();
            var context = DatabaseContext.Create(DB, name);
            Assert.IsTrue(File.Exists(name.CodeBase));
            Assert.IsTrue(new FileInfo(name.CodeBase).Length>0);
        }

        private DatabaseContext NewTestInstance()
        {
            return DatabaseContext.Create(DB, NewName());
        }

        private AssemblyName NewName()
        {
            return new AssemblyName("Gamlor.Tests.Dynamic");
        }
    }
}