using Db4objects.Db4o;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    class GenerateTestDatabase
    {
        [Test]
        public void GenerateTestDB()
        {
            using(var db = Db4oEmbedded.OpenFile("database.db4o"))
            {
                db.Store(new ClassWithoutFields());
                db.Store(new ClassWithFields());
                db.Store(new SubClass(){SubClassField = "testData.2",AField = "base-data"});
            }     
        }
        
    }
}