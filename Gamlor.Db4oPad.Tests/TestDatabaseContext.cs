using System.IO;
using System.Linq;
using Db4objects.Db4o;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class TestDatabaseContext : AbstractDatabaseFixture
    {
        private const string DataBaseFile = "withKnownTypes.db4o";

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
            Assert.IsTrue(types.Any(t => t.Text == "ClassWithoutFields"));
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
            var context = DatabaseContext.Create(DB, name,TypeLoader.Create(""));
            Assert.IsTrue(File.Exists(name.CodeBase));
            Assert.IsTrue(new FileInfo(name.CodeBase).Length>0);
        }
        [Test]
        public void WorksWithArrayTypes()
        {
            DB.Store(new SystemTypeArrays());
            var name = TestUtils.NewName();
            name.CodeBase = Path.GetTempFileName();
            var context = DatabaseContext.Create(DB, name, TypeLoader.Create(""));
            var type = context.MetaInfo
                .DyanmicTypesRepresentation
                .First(c => c.Key.Name.Equals(typeof (SystemTypeArrays).Name));
            Assert.NotNull(type);
        }
        [Test]
        public void NoEntryForArrays()
        {
            DB.Store(new SystemTypeArrays());
            var name = TestUtils.NewName();
            name.CodeBase = Path.GetTempFileName();
            var context = DatabaseContext.Create(DB, name,TestUtils.TestTypeResolver());
            var type = context.ListTypes();
            Assert.AreEqual(2,type.Count());
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
        [Test]
        public void LoadsFromAssembly()
        {
            CopyTestDB();
            using (var db = Db4oEmbedded.OpenFile(DataBaseFile))
            {
                var ctx = DatabaseContext.Create(db, TestUtils.NewName(),
                    TypeLoader.Create(@"..\..\Gamlor.Db4oPad.ExternalAssemblyForTests.dll"));
                var type = ctx.MetaInfo.EntityTypes.Single(t =>
                    t.TypeName.NameAndNamespace == "Gamlor.Db4oPad.ExternalAssemblyForTests.AType");
                Assert.IsTrue(type.KnowsType.HasValue);
                
            }
        }

        private DatabaseContext NewTestInstance()
        {
            var name = TestUtils.NewName();
            return DatabaseContext.Create(DB, name,TestUtils.TestTypeResolver());
        }
        private void CopyTestDB()
        {
            File.Delete(DataBaseFile);
            File.Copy("../../" + DataBaseFile, DataBaseFile);
        }
    }
}