using System;
using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o;
using Gamlor.Db4oPad.MetaInfo;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class ReadingMetaDataTestCases
    {

        [Test]
        public void ReadMixedGenericsRight()
        {
            RunTestWith("databaseWithGenerics.db4o",
                data=>
                    {
                        var listOfThings = (from td in data
                                           where td.Name == "ListOfThings"
                                           select td).Single();
                        var field = listOfThings.Fields.First();
                        var listOfInts = field.Type.TryResolveType((t) => typeof (int));
                        Assert.AreEqual(typeof(List<int>),listOfInts.Value);
                    });    
        }

        private void RunTestWith(string dbName, Action<IEnumerable<ITypeDescription>> context)
        {
            TestUtils.CopyTestDB(dbName);
            using (var ctx = Db4oEmbedded.OpenFile(dbName))
            {
                var metaData = MetaDataReader.Read(ctx,TypeLoader.Create(new string[0]));
                context(metaData);
            }
        }
        
    }
}