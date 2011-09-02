using System.Collections.Generic;
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
        public void ShowsNamespaces()
        {
            DB.Store(new TestTypes.ConflictingNamespaces.AClass());
            DB.Store(new TestTypes.ConflictingNamespaces.SubNamespace.AClass());
            var toTest = NewTestInstance();
            var types = toTest.ListTypes().ToList();
            Assert.IsTrue(types.Any(t => t.Text == "Gamlor.Db4oPad.Tests.TestTypes.ConflictingNamespaces"));
            Assert.IsTrue(types.Any(t => t.Text == "Gamlor.Db4oPad.Tests.TestTypes.ConflictingNamespaces.SubNamespace"));
        }
        [Test]
        public void SameNameDifferentAssembly()
        {
            var name = TestUtils.NewName();
            var cfg = Db4oEmbedded.NewConfiguration();
            cfg.File.ReadOnly = true;
            using(var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(cfg,"..\\..\\sameNameDifferentAssemblies.db4o"), name, TestUtils.TestTypeResolver()))
            {
                var types = ctx.ListTypes().ToList();
                Assert.IsTrue(types.Any());
            }
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
        public void FieldsHaveNiceName()
        {
            DB.Store(new ClassWithAutoProperty());
            var toTest = NewTestInstance();
            var types = toTest.ListTypes();
            Assert.IsTrue(types.Where(c => c.Children.Count > 0)
                .Any(c => c.Children.Single().Text == "AField:String"));
        }
        [Test]
        public void HasWrittenAssembly()
        {
            var name = TestUtils.NewName();
            name.CodeBase = Path.GetTempFileName();
            var context = DatabaseContext.Create(DB, name,TypeLoader.Create(new string[0]));
            Assert.IsTrue(File.Exists(name.CodeBase));
            Assert.IsTrue(new FileInfo(name.CodeBase).Length>0);
        }
        [Test]
        public void WorksWithArrayTypes()
        {
            DB.Store(new SystemTypeArrays());
            var name = TestUtils.NewName();
            name.CodeBase = Path.GetTempFileName();
            var context = DatabaseContext.Create(DB, name, TypeLoader.Create(new string[0]));
            var type = context.MetaInfo
                .DyanmicTypesRepresentation
                .First(c => c.Key.Name.Equals(typeof (SystemTypeArrays).Name));
            Assert.NotNull(type);
        }
        [Test]
        public void CanCreateNewInstances()
        {
            using(var toTest = NewTestInstance())
            {
                var newInstance = toTest.Query<ClassWithFields>().New();
                Assert.NotNull(newInstance);
                    
            }
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
                    TypeLoader.Create(new[]{@"..\..\Gamlor.Db4oPad.ExternalAssemblyForTests.dll"}));
                var type = ctx.MetaInfo.EntityTypes.Single(t =>
                    t.TypeName.NameAndNamespace == "Gamlor.Db4oPad.ExternalAssemblyForTests.AType");
                Assert.IsTrue(type.TryResolveType(TestUtils.FindNothingTypeResolver).HasValue);
                
            }
        }

        [Test]
        public void StoresObject()
        {
            using (var db = NewTestInstance())
            {
                var beforeInsert = db.Query<ClassWithoutFields>().Count();
                db.Store(new ClassWithoutFields());
                Assert.AreEqual(beforeInsert + 1, db.Query<ClassWithoutFields>().Count());
            }
        }
        [Test]
        public void DoNotFailOnStringFields()
        {
            using (var db = NewTestInstance())
            {
                var withStringField = new ClassWithFields {aField = "testData"};
                db.Store(withStringField);
            }
        }
        [Test]
        public void StoresCollectionsAlso()
        {
            using (var db = NewTestInstance())
            {
                var newInstance = new WithBuiltInGeneric()
                                      {
                                          AField = new List<string>()
                                      };
                db.Store(newInstance);
                newInstance.AField.Add("Fun");
                db.Store(newInstance);
                DB.Commit();
                using (var db2 = DB.Ext().OpenSession())
                {
                    var updated = db2.Query<WithBuiltInGeneric>().Single();
                    Assert.AreEqual(1, updated.AField.Count);
                }
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