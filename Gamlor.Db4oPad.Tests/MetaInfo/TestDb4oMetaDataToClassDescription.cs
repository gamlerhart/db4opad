using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.Reflect.Net;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using Moq;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestDb4oMetaDataToClassDescription
    {
        private const string FieldName = "aField";
        private IObjectContainer database;
        private IEnumerable<ITypeDescription> generatedClassses;
        

        [SetUp]
        public void Setup()
        {
            var dbContainer = MultiContainerMemoryDB.Create();
            using (var db = dbContainer.NewDB())
            {
                db.Store(new ClassWithoutFields());
                db.Store(new ClassWithFields());
                db.Store(new RecursiveClass());
                db.Store(new WithBuiltInGeneric());
                db.Store(new Generic<string>());
                db.Store(new Generic<string, List<string>>());
                db.Store(new Base());
                db.Store(new SubClass());
                db.Store(new ClassWithArrays());
                db.Store(new SystemTypeArrays());
            }
            database = dbContainer.NewDB();

            this.generatedClassses = MetaDataReader.Read(database,TestUtils.TestTypeResolver());
        }

        [Test]
        public void FindClassesFromAssembly()
        {
            this.generatedClassses = MetaDataReader.Read(database);
            var classMeta = For<ClassWithoutFields>();
            Assert.NotNull(classMeta);
            Assert.IsTrue(classMeta.KnowsType.HasValue);
            Assert.AreEqual(typeof(ClassWithoutFields),classMeta.KnowsType.Value);
        }

        [Test]
        public void ClassWithoutFields()
        {
            var classMeta = For<ClassWithoutFields>();
            Assert.NotNull(classMeta);
            Assert.IsFalse(classMeta.Fields.Any());
        }

        [Test]
        public void ClassWithFields()
        {
            var classMeta = For<ClassWithFields>();
            Assert.NotNull(classMeta);
            Assert.AreEqual(FieldName, classMeta.Fields.Single().Name);
            Assert.AreEqual("String", classMeta.Fields.Single().Type.Name);
        }

        [Test]
        public void TypeMapIncludesTypesOfFields()
        {
            var classMeta = For<string>();
            Assert.NotNull(classMeta);
        }
        [Test]
        public void CanHaveRecursiveType()
        {
            var classMeta = For<RecursiveClass>();
            Assert.NotNull(classMeta);
            Assert.AreEqual(FieldName, classMeta.Fields.Single().Name);
            Assert.AreEqual(classMeta, classMeta.Fields.Single().Type);
        }
        [Test]
        public void WithBuiltInGeneric()
        {
            var classMeta = For<WithBuiltInGeneric>();
            Assert.NotNull(classMeta);
            Assert.AreEqual(FieldName, classMeta.Fields.Single().Name);
            Assert.AreEqual(typeof(List<string>).Name, classMeta.Fields.Single().Type.Name);
        }
        [Test]
        public void SimpleGeneric()
        {
            var classMeta = For<Generic<string>>();
            Assert.NotNull(classMeta);
            Assert.AreEqual(FieldName, classMeta.Fields.Single().Name);
            Assert.AreEqual(typeof(List<string>).Name, classMeta.Fields.Single().Type.Name);
        }
        [Test]
        public void NestedGeneric()
        {
            var classMeta = For<Generic<string, List<string>>>();
            Assert.NotNull(classMeta);
            Assert.AreEqual(FieldName, classMeta.Fields.Single().Name);
            Assert.AreEqual(typeof(Dictionary<string, List<string>>).Name, classMeta.Fields.Single().Type.Name);
        }
        [Test]
        public void BaseClass()
        {
            var classMeta = For<Base>();
            Assert.NotNull(classMeta);
            Assert.AreEqual(FieldName, classMeta.Fields.Single().Name);
            Assert.AreEqual(typeof(string).Name, classMeta.Fields.Single().Type.Name);
        }
        [Test]
        public void SubClass()
        {
            var classMeta = For<SubClass>();
            Assert.NotNull(classMeta);
            Assert.IsTrue(classMeta.Fields.Any(f => f.Name == "subClassField"));
            Assert.IsTrue(classMeta.BaseClass.Fields.Any(f => f.Name == FieldName));
        }
        [Test]
        public void TypeWithArrays()
        {
            var classMeta = For<ClassWithArrays>();
            Assert.NotNull(classMeta);
            Assert.IsTrue(classMeta.Fields.Single(f=>f.Name=="strings").Type.IsArray);
            Assert.IsTrue(classMeta.Fields.Single(f => f.Name == "withfields").Type.IsArray);
        }
        [Test]
        public void TypeWithSystemArrays()
        {
            var classMeta = For<SystemTypeArrays>();
            Assert.NotNull(classMeta);
            var fieldInfo = classMeta.Fields.Single(f => f.Name == "aField").Type;
            Assert.IsTrue(fieldInfo.IsArray);
            Assert.IsTrue(fieldInfo.ArrayOf.Value.KnowsType.HasValue);
        }

        [Test]
        public void CanHandleSameTypeTwiceScenario()
        {
            var contex = MultiContainerMemoryDB.Create();
            StoreAPerson(contex);
            StoreAPerson(contex);
            using (var db = contex.NewDB())
            {
                this.generatedClassses = MetaDataReader.Read(db);
                Assert.IsTrue(generatedClassses.Any());
            }

        }

        [Test]
        public void DBWithoutAllTypesInList()
        {
            var reflector = new NetReflector();
            var colorHolderType = reflector.ForClass(typeof (ColorHolder));

            var dbMock = new Mock<IObjectContainer>();
            dbMock.Setup(c => c.Ext().KnownClasses()).Returns(new[] {colorHolderType});
            var classInfos = MetaDataReader.Read(dbMock.Object, TestUtils.TestTypeResolver());
            Assert.IsTrue(1<classInfos.Count());
        }

        private void StoreAPerson(MultiContainerMemoryDB contex)
        {
            using (var container = contex.NewDB())
            {
                container.Store(new Person());
            }
        }

        private ITypeDescription For<T>()
        {
            return generatedClassses
                .Where(c=>!c.Name.EndsWith("[]"))
                .Single(c => c.Name.StartsWith(typeof(T).Name.Replace('`', '_')));
        }
    }

}