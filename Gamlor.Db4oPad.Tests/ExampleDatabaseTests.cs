using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                                Assert.AreEqual(typeof (List<>), propertyType.GetGenericTypeDefinition());
                            });
        }
        [Test]
        public void CanOpenEnums()
        {
            RunTestWith("databaseWithEnum.db4o",
                        ctx =>
                        {
                            var arrayType = ctx.MetaInfo.DyanmicTypesRepresentation
                                .Single(t => t.Key.Name.Contains("ItemWithEnum")).Value;
                            var propertyType = arrayType.GetField("enumField").FieldType;
                            Assert.IsTrue(typeof(Enum).IsAssignableFrom(propertyType));
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
                                Assert.AreEqual(typeof (string), propertyType);
                                Assert.IsNotNull(property);
                            });
        }

        [Test]
        public void CanOpenDBWithNestedGenerics()
        {
            RunTestWith("databaseWithNestedGenerics.db4o",
                        ctx =>
                            {
                                var outerType = ctx.MetaInfo.DyanmicTypesRepresentation
                                    .Any(t => t.Key.Name.Contains("NestedGenerics_1_String"));
                                var innerType = ctx.MetaInfo.DyanmicTypesRepresentation
                                    .Any(t => t.Key.Name.Contains("NestedGenerics_1_InnerGeneric_2_ListItem_String_ListItem"));
                                Assert.IsTrue(outerType);
                                Assert.IsTrue(innerType);
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

        [Test]
        public void WorksWithMixedGenerics()
        {
            RunTestWith("databaseWithKnownGenricsAndUnknowParameterTypes.db4o",
                        ctx =>
                            {
                                var context = ctx.MetaInfo.DataContext;
                                dynamic metaData = context.GetProperty("ListHolder").GetValue(null, null);
                                Assert.NotNull(metaData);
                            });
        }

        [Test]
        public void WorksWithDictionary()
        {
            RunTestWith("databaseWithDictionary.db4o",
                        ctx =>
                            {
                                var context = ctx.MetaInfo.DataContext;
                                dynamic metaData = context.GetProperty("DictionaryHolder").GetValue(null, null);
                                Assert.NotNull(metaData);
                            });
        }

        [Test]
        public void WorksWithVirtualFields()
        {
            RunTestWith("virtualFields.db4o",
                        ctx =>
                            {
                                var context = ctx.MetaInfo.DataContext;
                                dynamic metaData = context.GetProperty("Location").GetValue(null, null);
                                Assert.NotNull(metaData);
                            });
        }
        [Test]
        public void WorksWithSameNameInDifferentAssemblies()
        {
            RunTestWith("sameNameDifferentAssemblies.db4o",
                        ctx =>
                        {
                            var context = ctx.MetaInfo.DataContext;
                            dynamic ns = context.GetProperty("Db4oPad").GetValue(null, null);
                            dynamic metaData1 = ns.TestDBs.SameNameInDifferentAssemblies_Db4oPad_TestDBs;
                            dynamic metaData2 = ns.TestDBs.SameNameInDifferentAssemblies_Db4oPad_TestDBs_OtherAssembly;
                            Assert.NotNull(metaData1);
                            Assert.NotNull(metaData2);
                            IEnumerable enumerable = metaData2;
                            Assert.IsTrue(0<enumerable.Cast<object>().Count());
                        });
        }

        [Test]
        public void WorksWithNameCollisions()
        {
            RunTestWith("dataspaceWithNameCollisions.db4o",
                        ctx =>
                            {
                                var context = ctx.MetaInfo.DataContext;
                                dynamic metaData = context.GetProperty("Db4oPad").GetValue(null, null);
                                {
                                    var type = metaData.TestDBs.SameNameInDifferentNamespaces;
                                    Assert.NotNull(type);
                                    object objInstance1 = type.New();
                                    Assert.AreEqual(0, objInstance1.GetType().GetProperties().Count());
                                }
                                {
                                    var type = metaData.TestDBs.NameSpaceOne.SameNameInDifferentNamespaces;
                                    Assert.NotNull(type);
                                    object objInstance1 = type.New();
                                    Assert.NotNull(objInstance1.GetType().GetProperty("Field1"));
                                }
                                {
                                    var type = metaData.TestDBs.NameSpaceTwo.SameNameInDifferentNamespaces;
                                    Assert.NotNull(type);
                                    object objInstance1 = type.New();
                                    Assert.NotNull(objInstance1.GetType().GetProperty("Field2"));
                                }
                            });
        }

        private void RunTestWith(string dbName, Action<DatabaseContext> context)
        {
            TestUtils.CopyTestDB(dbName);
            var metaInfo = GetConfig(dbName);
            var cfg = Db4oEmbedded.NewConfiguration();
            cfg.File.ReadOnly = true;
            metaInfo.Item1.Configure(cfg);
            var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(cfg,dbName),metaInfo.Item2);
            CurrentContext.NewContext(ctx);
            try
            {
                context(ctx);
            }
            finally
            {
                CurrentContext.CloseContext();
            }
        }

        private static Tuple<DatabaseConfigurator, DatabaseMetaInfo> GetConfig(string dbName)
        {
            var cfg = Db4oEmbedded.NewConfiguration();
            cfg.File.ReadOnly = true;
            using (var metaInfoDB = Db4oEmbedded.OpenFile(cfg,dbName))
            {
                var name = TestUtils.NewName();
                var meta = DatabaseMetaInfo.Create(metaInfoDB, TypeLoader.Create(new string[0]), name);
                var config = DatabaseConfigurator.Create(meta);
                return Tuple.Create(config, meta);
            }
        }
    }
}