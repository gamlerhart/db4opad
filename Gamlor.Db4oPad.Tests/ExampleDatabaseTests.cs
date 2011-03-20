using System.IO;
using System.Reflection;
using Db4objects.Db4o;
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
            using(var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(Databasename),name))
            {
                var meta = ctx.MetaInfo;
                Assert.NotNull(meta);
            }
            using (var ctx = DatabaseContext.Create(Db4oEmbedded.OpenFile(Databasename), Assembly.LoadFrom(name.CodeBase)))
            {
                var meta = ctx.MetaInfo;
                Assert.NotNull(meta);
            }
        }

        private void CopyTestDB()
        {
            File.Delete(Databasename);
            File.Copy("../../" + Databasename, Databasename);
        }
        
    }
}