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
        private IMetaInfo toTest;
        private readonly string className = typeof (Person).Name;
        private readonly string classNameSpace = typeof (Person).Namespace + "." + typeof (Person).Name;


        protected override void FixtureSetup(IObjectContainer db)
        {
            db.Store(new Person("Roman", "Stoffel", 24));
            this.toTest = MetaInfoProvider.Create(DatabaseMetaInfo.Create(db,
                                                                          new AssemblyName("Gamlor.Dynamic")
                                                                              {
                                                                                  CodeBase = Path.GetTempFileName()
                                                                              }));
        }

        [Test]
        public void ListsMetaInfo()
        {
            var classes = toTest.Classes;
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
            var classes = toTest.Classes;
            Assert.AreEqual(className, classes.Single().ToString());
            Assert.AreEqual(className, classes.Single().Name);
            Assert.AreEqual(classNameSpace, classes.Single().FullName);
        }

        [Test]
        public void CanReuseAssembly()
        {
            var theClass = QueryForPersonClass(this.toTest);
            var result = MetaInfoProvider.Create(DatabaseMetaInfo.Create(DB, theClass.Assembly));
            var theClassAsSencondTime = QueryForPersonClass(result);
            Assert.AreEqual(theClass.Assembly,theClassAsSencondTime.Assembly);
            Assert.AreEqual(theClass,theClassAsSencondTime);
        }

        private Type QueryForPersonClass(IMetaInfo theInfoSource)
        {
            return (from t in theInfoSource.DyanmicTypesRepresentation.Values
                    where t.Name.Contains("Person")
                    select t).Single();
        }
    }
}