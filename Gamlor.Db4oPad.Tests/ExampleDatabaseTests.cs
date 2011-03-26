using System.IO;
using System.Reflection;
using Db4objects.Db4o;
using Gamlor.Db4oPad.MetaInfo;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class ExampleDatabaseTests
    {
        private const string Databasename = "carExampleDB.db4o";

        [Test]
        public void CanOpenCarExample()
        {
            CopyTestDB();
            var name = TestUtils.NewName();
            DatabaseMetaInfo meta = null;
            using(var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(Databasename),name,TypeLoader.Create("")))
            {
                meta = ctx.MetaInfo;
                Assert.NotNull(meta);
            }
            using (var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(Databasename), meta))
            {
                var meta2 = ctx.MetaInfo;
                Assert.NotNull(meta2);
            }
        }

        private void CopyTestDB()
        {
            File.Delete(Databasename);
            File.Copy("../../" + Databasename, Databasename);
        }
        
    }
}