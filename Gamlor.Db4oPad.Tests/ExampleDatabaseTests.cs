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

        private void RunTestWith(string dbName, Action<DatabaseContext> context)
        {
            TestUtils.CopyTestDB(dbName);
            var name = TestUtils.NewName();
            using (var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(dbName), name,
                                                    TypeLoader.Create(new string[0])))
            {
                context(ctx);
            }
        }
    }
}