using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
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
            this.database = MemoryDBForTests.NewDB();


            database.Store(new ClassWithoutFields());
            database.Store(new ClassWithFields());
            database.Store(new RecursiveClass());
            database.Store(new WithBuiltInGeneric());
            database.Store(new Generic<string>());
            database.Store(new Generic<string, List<string>>());
            database.Store(new Base());
            database.Store(new SubClass());

            this.generatedClassses = MetaDataReader.Read(database);
        }


        [Test]
        public void ClassWithoutFields()
        {
            var classMeta = For<ClassWithoutFields>();
            Assert.NotNull(classMeta);
            Assert.IsFalse(classMeta.Fields.Any());
        }
        [Test]
        public void FullNameSimpleClass()
        {
            var classMeta = For<ClassWithoutFields>();
            var type = typeof(ClassWithoutFields);
            var assembly = type.Assembly.GetName().Name;
            Assert.AreEqual(typeof(ClassWithoutFields).FullName, classMeta.TypeName.NameWithGenerics);
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

        private void StoreAPerson(MultiContainerMemoryDB contex)
        {
            using (var container = contex.NewDB())
            {
                container.Store(new Person());
            }
        }

        private ITypeDescription For<T>()
        {
            return generatedClassses.Where(c => c.Name.Equals(typeof(T).Name.Replace('`','_'))).Single();
        }
    }

}