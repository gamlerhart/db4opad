using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Db4objects.Db4o;
using Gamlor.Db4oExt.IO;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oExt.Tests.IO
{
    [TestFixture]
    class HighLevelIOTestCases
    {
        private IEmbeddedObjectContainer container;
        private const int ObjectsPerSession = 200;

        [SetUp]
        public void Setup()
        {
            OpenContainer();
        }

        [Test]
        public void OpenClose()
        {
            container.Dispose();
            OpenContainer();
            container.Dispose();
        }

        [Test]
        public void StoreAndCommit()
        {
            container.Store(new Person("Roman","Stoffel",24));      
            container.Store(new Person("Joe","Cool",42));  
            container.Commit();
        }

        [Test]
        public void StoreAndQuery()
        {
            container.Store(new Person("Roman", "Stoffel", 24));
            container.Store(new Person("Joe", "Cool", 42));

            var persons = container.Query<Person>();
            Assert.AreEqual(2,persons.Count);
        }



        [Test]
        public void BashingDB()
        {
            var amoutOfTasks = Environment.ProcessorCount*2;
            Func<int> bashAction = DoBashDB;

            var tasks = new List<Task<int>>();
            for(int i=0;i<amoutOfTasks;i++)
            {
                var task = new Task<int>(bashAction);
                task.Start();
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());

            var persons = container.Query<Person>();
            Assert.AreEqual(amoutOfTasks * ObjectsPerSession, persons.Count);
        }

        int DoBashDB()
        {
            using(var session = container.Ext().OpenSession())
            {
                for(var i=0;i<ObjectsPerSession;i++)
                {
                    session.Store(new Person("Person"+i,"name"+i,i));
                    if(i%4==0)
                    {
                        var result = session.Query<Person>().ToList();
                    }
                }
                return session.Query<Person>().Count;
            }
        }

        [TearDown]
        public void TearDown()
        {
            container.Dispose();   
        }


        
        private void OpenContainer()
        {
            var config = Db4oEmbedded.NewConfiguration();
            config.File.Storage = new AggressiveCacheStorage();
            this.container = Db4oEmbedded.OpenFile(config, Path.GetRandomFileName());
        }
    }
}