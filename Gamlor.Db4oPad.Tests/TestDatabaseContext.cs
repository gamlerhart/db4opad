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
            db.Store(new SystemTypeArrays());
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
                .Any(c => c.Children.Single().Text == "aField:String"));
        }
        [Test]
        public void HasWrittenAssembly()
        {
            var name = TestUtils.NewName();
            name.CodeBase = Path.GetTempFileName();
            var context = DatabaseContext.Create(DB, name);
            Assert.IsTrue(File.Exists(name.CodeBase));
            Assert.IsTrue(new FileInfo(name.CodeBase).Length>0);
        }
        [Test]
        public void QueryForTypeWithSystemArray()
        {
            var name = TestUtils.NewName();
            name.CodeBase = Path.GetTempFileName();
            var context = DatabaseContext.Create(DB, name);
            var type = context.MetaInfo
                .DyanmicTypesRepresentation
                .First(c => c.Key.Name.Equals(typeof (SystemTypeArrays).Name));
            Assert.NotNull(type);
        }
        [Test]
        public void DisposesDB()
        {
            NewTestInstance().Dispose();
            Assert.IsTrue(DB.Ext().IsClosed());
        }
        [Test]
        public void CanQuery()
        {
            using (var db = NewTestInstance())
            {
                var result = from i in db.Query<ClassWithoutFields>()
                             select i;
                Assert.AreNotEqual(0,result.Count());
            }
        }

        private DatabaseContext NewTestInstance()
        {
            var name = TestUtils.NewName();
            return DatabaseContext.Create(DB, name);
        }
    }
}