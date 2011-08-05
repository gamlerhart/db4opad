using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    public class ExampleDatabasesWithKnownTypesTests
    {
        [Test]
        public void CanDealWithGenerics()
        {
            var containerStorage = MultiContainerMemoryDB.Create();
            using (var container = containerStorage.NewDB())
            {
                container.Store(new WithGenericDictionary());
            }
            using (var container = containerStorage.NewDB(c => { c.File.ReadOnly = true; }))
            {
                var ctx = DatabaseContext.Create(container, TestUtils.NewName(),
                                                 TypeLoader.Create(new string[0]));
                CurrentContext.NewContext(ctx);
                try
                {
                    var queryContext = (IQueryable<WithGenericDictionary>) ctx.MetaInfo.DataContext.
                        GetProperty("ExampleDatabasesWithKnownTypesTests_WithGenericDictionary").GetValue(null,null);


                    Assert.AreEqual(1, queryContext.Count());
                }
                finally
                {
                    CurrentContext.CloseContext();
                }
            }
        }

        public class WithGenericDictionary
        {
            private IDictionary<string, ClassWithoutFields> field = new Dictionary<string, ClassWithoutFields>();
        }
    }
}