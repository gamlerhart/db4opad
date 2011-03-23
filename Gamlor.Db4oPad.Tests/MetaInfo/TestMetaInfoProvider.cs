using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestMetaInfoProvider : AbstractDatabaseFixture
    {
        private DatabaseMetaInfo toTest;
        private readonly string className = typeof (Person).Name;
        private readonly string classNameSpace = typeof (Person).Namespace + "." + typeof (Person).Name;


        protected override void FixtureSetup(IObjectContainer db)
        {
            db.Store(new Person("Roman", "Stoffel", 24));
            this.toTest = DatabaseMetaInfo.Create(db,new AssemblyName("Gamlor.Dynamic")
                                                                              {
                                                                                  CodeBase = Path.GetTempFileName()
                                                                              });
        }

        [Test]
        public void ListsMetaInfo()
        {
            var classes = toTest.Types;
            Assert.AreEqual(1, classes.Count());
        }

        [Test]
        public void ProvidesQueryClass()
        {
            var dataContext = toTest.DataContext;
            Assert.NotNull(dataContext);
            Assert.NotNull(CodeGenerator.QueryContextClassName, dataContext.Name);
        }
        [Test]
        public void PrintsItself()
        {
            var printLabel = toTest.ToString();
            var expectedText = "Meta-Info";
            Assert.AreEqual(expectedText, printLabel);
        }

        [Test]
        public void NamesClasses()
        {
            var classes = toTest.Types;
            Assert.AreEqual(className, classes.Single().ToString());
            Assert.AreEqual(className, classes.Single().Name);
            Assert.AreEqual(classNameSpace, classes.Single().TypeName);
        }

        [Test]
        public void CanReuseAssembly()
        {
            var theClass = this.toTest.DataContext;
            var result = DatabaseMetaInfo.Create(DB, theClass.Assembly);
            var theClassAsSencondTime = QueryForPersonClass(result);
            Assert.AreEqual(theClass.Assembly,theClassAsSencondTime.Assembly);
            Assert.AreEqual(theClass,theClassAsSencondTime);
        }

        private Type QueryForPersonClass(DatabaseMetaInfo theInfoSource)
        {
            return (from t in theInfoSource.DyanmicTypesRepresentation.Values
                    where t.Name.Contains("Person")
                    select t).Single();
        }
    }
}