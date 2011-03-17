using System;
using System.IO;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Reflect.Net;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;
using Db4objects.Db4o.Linq;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class TestDb4oAssumptions : AbstractDatabaseFixture
    {
        protected override void ConfigureDb(IEmbeddedConfiguration configuration)
        {
            
        }

        [Test]
        public void Aliasing()
        {
            DB.Store(new ClassWithFields());

            Reopen(c => c.Common.AddAlias(
                new TypeAlias(
                    ReflectPlatform.FullyQualifiedName(typeof (ClassWithFields)),
                    ReflectPlatform.FullyQualifiedName(typeof (Base))
                    )));

            var result = from b in DB.AsQueryable<Base>()
                             select b;
            Assert.AreEqual(1,result.Count());

        }

        [Test]
        public void RootClass()
        {
            var db4oReflector = new NetReflector();
            var baseClass= db4oReflector.ForClass(typeof(ClassWithoutFields)).GetSuperclass();
            var baseClassOfBaseClass = baseClass.GetSuperclass();
            Assert.NotNull(baseClass);
            Assert.IsNull(baseClassOfBaseClass);
        }

//        [Test]
//        public void Aliasing2()
//        {
//            File.Delete("testDB.db4o");
//            File.Copy("../../testDB.db4o", "testDB.db4o");
//            var cfg = Db4oEmbedded.NewConfiguration();
//            var dynamicReflector = DynamicGeneratedTypesReflector.CreateInstance();
//            dynamicReflector.AddType("ConsoleApplication1.MyData, Db4oDoc", typeof(MyData2));
//            dynamicReflector.AddType(ReflectPlatform.FullyQualifiedName(typeof(MyData2)), typeof(MyData2));
            //cfg.Common.ReflectWith(dynamicReflector);
//            var db = Db4oEmbedded.OpenFile(cfg, @"testDB.db4o");
//            db.Store(new MyData2(11));
//            db.Commit();
//            var query = db.Query();
//            query.Constrain(typeof (MyData2));
//            query.Descend("value").Constrain(-11).Greater();
//            var r = query.Execute();
//            var count = r.Count;
//
//            var result = from b in db.AsQueryable<MyData2>()
//                         where b.Value>=-5
//                         select b;
//            Assert.AreEqual(1, result.Count());
//
//        }
        class MyData2
        {
            public MyData2(int number)
            {
                Value = number;
            }

            public void Foo()
            {
                Value = 22;
            }

            private int value;
            public int Value
            {
                get { return value; }
                private set { this.value = value; }
            }
        }   
    }
}