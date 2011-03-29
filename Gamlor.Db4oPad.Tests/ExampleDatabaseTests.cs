using System.IO;
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
        private const string ArrayTypeDatabase = "databaseWithArrayFields.db4o";

        [Test]
        public void CanOpenCarExample()
        {
            TestUtils.CopyTestDB(CarDatabase);
            var name = TestUtils.NewName();
            DatabaseMetaInfo meta = null;
            using(var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(CarDatabase),name,TypeLoader.Create(new string[0])))
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
            TestUtils.CopyTestDB(ArrayTypeDatabase);
            var name = TestUtils.NewName();
            using (var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(ArrayTypeDatabase), name,
                TypeLoader.Create(new string[0])))
            {
                var arrayType = ctx.MetaInfo.DyanmicTypesRepresentation
                    .Single(t => t.Key.Name.Contains("Location") && !t.Key.IsArray).Value;
                var propertyType = arrayType.GetProperty("AlternativeLocations").PropertyType;
                Assert.IsTrue(propertyType.IsArray);
            }
        }
        
    }
}