using System;
using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o;
using Gamlor.Db4oPad.MetaInfo;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class ExampleDatabaseTests
    {
        private const string CarDatabase = "carExampleDB.db4o";

        [Test]
        public void CanOpenCarExample()
        {
            TestUtils.CopyTestDB(CarDatabase);
            var name = TestUtils.NewName();
            DatabaseMetaInfo meta = null;
            using (
                var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(CarDatabase), name,
                                                 TypeLoader.Create(new string[0])))
            {
                meta = ctx.MetaInfo;
                Assert.NotNull(meta);
            }
            using (var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(CarDatabase), meta))
            {
                var meta2 = ctx.MetaInfo;
                Assert.NotNull(meta2);
            }
        }
        [Test]
        public void CanCreateNewInstance()
        {
            RunTestWith(CarDatabase,
                        ctx =>
                            {
                                var propertyInfo = ctx.MetaInfo.DataContext.GetProperty("Car");
                                dynamic carQuery = propertyInfo.GetValue(null, null);
                                 var newCar = carQuery.New();
                                 Assert.NotNull(propertyInfo.PropertyType.GetMethod("New"));
                                Assert.NotNull(newCar);
                            });
        }

        [Test]
        public void CanOpenArrayDatabase()
        {
            RunTestWith("databaseWithArrayFields.db4o",
                        ctx =>
                            {
                                var arrayType = ctx.MetaInfo.DyanmicTypesRepresentation
                                    .Single(t => t.Key.Name.Contains("Location") && !t.Key.IsArray).Value;
                                var propertyType = arrayType.GetProperty("AlternativeLocations").PropertyType;
                                Assert.IsTrue(propertyType.IsArray);
                            });
        }

        [Test]
        public void CanOpenGenericsDatabase()
        {
            RunTestWith("databaseWithGenerics.db4o",
                        ctx =>
                        {
                            var arrayType = ctx.MetaInfo.DyanmicTypesRepresentation
                                .Single(t => t.Key.Name.Contains("ListOfThings")).Value;
                            var propertyType = arrayType.GetField("listOfFood").FieldType;
                            Assert.AreEqual(typeof(List<>),propertyType.GetGenericTypeDefinition());
                        });
        }
        [Test]
        public void CanOpenDBWithNestedClasses()
        {
            RunTestWith("databaseWithNestedClasses.db4o",
                        ctx =>
                        {
                            var arrayType = ctx.MetaInfo.DyanmicTypesRepresentation
                                .Single(t => t.Key.Name.Contains("NestedClasses_ChildClass")).Value;
                            var propertyType = arrayType.GetField("data").FieldType;
                            var property = ctx.MetaInfo.DataContext.GetProperty("NestedClasses_ChildClass");
                            Assert.AreEqual(typeof(string), propertyType);
                            Assert.IsNotNull(property);
                        });
        }
        [Test]
        public void HasIndexInfo()
        {
            RunTestWith("databaseWithIndexes.db4o",
                        ctx =>
                        {
                            var context = ctx.MetaInfo.DataContext;
                            dynamic metaData = context.GetProperty("MetaData").GetValue(null, null);
                            Assert.IsTrue(metaData.IndexesOnFields.firstIndexedField.ToString().Contains("Indexed)"));
                            Assert.IsTrue(metaData.IndexesOnFields.notIndexed.ToString().Contains("NotIndexed)"));
                        });
        }

        private void RunTestWith(string dbName, Action<DatabaseContext> context)
        {
            TestUtils.CopyTestDB(dbName);
            var name = TestUtils.NewName();
            var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(dbName), name,
                                                         TypeLoader.Create(new string[0]));
            CurrentContext.NewContext(ctx);
            try
            {
                context(ctx);
                    
            }finally
            {
                CurrentContext.CloseContext();
            }
        }
    }
}