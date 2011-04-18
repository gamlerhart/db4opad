using System;
using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class TestPadExtensions
    {
        private MultiContainerMemoryDB testDBs;
        private IList<Person> originalData;

        [SetUp]
        public void Setup()
        {
            originalData = new List<Person>()
                               {
                               new Person("Roman","Stoffel",42),    
                               new Person("Roman","Stoffel",33),
                               new Person("Tom","Cool",33)
                               };
            this.testDBs = MultiContainerMemoryDB.Create();
            using(var db = testDBs.NewDB())
            {
                foreach (var person in originalData)
                {
                    db.Store(person);
                }
            }
        }
        
        [Test]
        public void UpdateEmptyCollection()
        {
            TestUtils.WithTestContext(testDBs.NewDB(), TestUtils.DefaultResolver(),
                ()=>
                    {
                        var persons = from p in CurrentContext.Query<Person>()
                                          where p.Age > 100
                                          select p;
                        persons.UpdateAll(p => p.FirstName = "New Me");
                        AssertNoChance(CurrentContext.Query<Person>());
                    });   
        }
        [Test]
        public void ReturnsQueryResult()
        {
            TestUtils.WithTestContext(testDBs.NewDB(), TestUtils.DefaultResolver(),
                () =>
                {
                    var persons = from p in CurrentContext.Query<Person>()
                                  where p.Age > 40
                                  select p;
                    var result = persons.UpdateAll(p => p.FirstName = "New Me");
                    Assert.AreEqual(1,result.Count());
                });
        }
        [Test]
        public void UpdatesSingle()
        {
            TestUtils.WithTestContext(testDBs.NewDB(), TestUtils.DefaultResolver(),
                () =>
                {
                    var persons = from p in CurrentContext.Query<Person>()
                                  where p.Age > 40
                                  select p;
                    persons.UpdateAll(p => p.FirstName = "New Me");
                    var updated = from p in CurrentContext.Query<Person>()
                                  where p.FirstName == "New Me"
                                  select p;
                    Assert.AreEqual(1, updated.Count());
                });
        }
        [Test]
        public void UpdatesABunch()
        {
            TestUtils.WithTestContext(testDBs.NewDB(), TestUtils.DefaultResolver(),
                () =>
                {
                    var persons = from p in CurrentContext.Query<Person>()
                                  where p.Age > 10
                                  select p;
                    persons.UpdateAll(p => p.FirstName = "New Me");
                    var updated = from p in CurrentContext.Query<Person>()
                                  where p.FirstName == "New Me"
                                  select p;
                    Assert.AreEqual(3, updated.Count());
                });
        }

        private void AssertNoChance(ExtendedQueryable<Person> query)
        {
            Assert.IsTrue(query.All(p=>originalData.Contains(p)));
                        
        }
    }
}