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
        private const string OldName = "Gamlor.Db4oPad.Tests.OtherData.MyData, Gamlor.Db4oPad.Tests";
        private const string Databasename = "testDB.db4o";

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

        [Test]
        public void AliasingWithoutTypes()
        {
            CopyTestDB();
            var cfg = Db4oEmbedded.NewConfiguration();
            var dynamicReflector = DynamicGeneratedTypesReflector.CreateInstance(new NetReflector());
            dynamicReflector.AddType(ReflectPlatform.FullyQualifiedName(typeof(MyData2)), typeof(MyData2));
            cfg.Common.ReflectWith(dynamicReflector);
            cfg.Common.AddAlias(new TypeAlias(OldName, ReflectPlatform.FullyQualifiedName(typeof(MyData2))));
            var db = Db4oEmbedded.OpenFile(cfg, Databasename);

            var result = from b in db.AsQueryable<MyData2>()
                         where b.Value>=-5
                         select b;
            Assert.AreEqual(1, result.Count());

        }

        private void CopyTestDB()
        {
            File.Delete(Databasename);
            File.Copy("../../" + Databasename, Databasename);
        }

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