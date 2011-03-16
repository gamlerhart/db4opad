using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.IO;
using Db4objects.Db4o.Linq;
using Gamlor.Db4oExt.IO;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oExt.Tests.IO
{
    public class MiniBenchMark
    {
        private const int AmountOfRuns = 100;


//        [Test]
        public void TheBenchMark()
        {
            RunRoundWith(DefaultConfig(),"default warmup ");
            RunRoundWith(OurImplementation(), "our warmup ");


            RunRoundWith(DefaultConfig(), "default real ");
            RunRoundWith(OurImplementation(), "our real ");
        }

        private void RunRoundWith(Func<IEmbeddedConfiguration> config, string label)
        {
            TimeRun(()=>TheBenchmarkRound(config),label+" run 1");        
            TimeRun(()=>TheBenchmarkRound(config),label+" run 2");        
            TimeRun(()=>TheBenchmarkRound(config),label+" run 3");        
        }

        private void TheBenchmarkRound(Func<IEmbeddedConfiguration> config)
        {
            var file = Path.GetTempFileName();
            using (var container = Db4oEmbedded.OpenFile(config(),file))
            {
                DoQuerys(container.Ext().OpenSession);
                var runOne = new Task(() => DoQuerys(container.Ext().OpenSession));
                runOne.Start();
                var runTwo = new Task(() => DoQuerys(container.Ext().OpenSession));
                runTwo.Start();
                var runThree = new Task(() => DoQuerys(container.Ext().OpenSession));
                runThree.Start();

                DoQuerys(container.Ext().OpenSession);
                
                Task.WaitAll(runOne, runTwo, runThree);
            }
            File.Delete(file);
        }

        private void DoQuerys(Func<IObjectContainer> containerFac)
        {
            using(var container = containerFac() )
            {
                for (int i = 0; i < AmountOfRuns; i++)
                {
                    container.Store(new Person("Roman", "Stoffel", 24));
                    container.Store(new Person("Joe", "Awesome", 55));
                    container.Store(new WithBuiltInGeneric() { AField = new List<string>() { "1", "2", "3", "LONG" } });


                    EagerEvaluate(from Person p in container
                                 where p.Age > 22 
                                    select p);

                    

                    EagerEvaluate(from WithBuiltInGeneric p in container
                                  select p);

                    EagerEvaluate(from Person p in container
                                  where p.FirstName=="Roman"
                                  select p);

                    container.Store(new Person("Other Guy"+i, "ThatGui", i));


                    var guyName = "Other Guy" + i;
                    Update(from Person p in container
                           where p.FirstName == guyName
                                  select p,container);


                    EagerEvaluate(from Person p in container
                                  where p.FirstName == "Joe"
                                  select p);
                    container.Commit();
                }
            }
        }

        private void Update(IEnumerable<Person> guys,IObjectContainer db)
        {
            foreach (var person in guys)
            {
                person.Sirname = person.Sirname + "New Part";
                db.Store(person);
            }
        }

        private int EagerEvaluate<T>(IEnumerable<T> enumerable)
        {
            var result = enumerable.ToList();
            return result.Count;
        }

        private void TimeRun(Action toTime, string label)
        {
            var ts = Stopwatch.StartNew();
            toTime();
            Console.Out.WriteLine("The round {0} used {1}ms time",label,ts.ElapsedMilliseconds);
        }

        private Func<IEmbeddedConfiguration> DefaultConfig()
        {
            return () =>
                       {
                           var config = Db4oEmbedded.NewConfiguration();
                           config.Common.ObjectClass(typeof (Person)).ObjectField("firstName").Indexed(true);
                           config.Common.ObjectClass(typeof (Person)).ObjectField("age").Indexed(true);
                           config.File.Storage = new CachingStorage(new FileStorage());
                           return config;
                       };
        }
        private Func<IEmbeddedConfiguration> InMemory()
        {
            return () =>
                       {
                           var cfg = DefaultConfig()();
                           cfg.File.Storage = new PagingMemoryStorage();
                           return cfg;
                       };
        }
        private Func<IEmbeddedConfiguration> OurImplementation()
        {
            return () =>
            {
                var cfg = DefaultConfig()();
                cfg.File.Storage = AggressiveCacheStorage.RegularStorage();
                return cfg;
            };
        }
    }
}