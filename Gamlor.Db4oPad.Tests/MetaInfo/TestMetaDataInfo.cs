using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestMetaDataInfo : TypeGenerationBase
    {
        [Test]
        public void HasMetaDataProperty()
        {

            object dataContext = GetDataContext(TestMetaData.CreateEmptyClassMetaInfo());
            var metaData = GetMetaDataProperty(dataContext);
            Assert.IsNotNull(metaData);
        }

        [Test]
        public void CanGetMetaData()
        {
            object metaData = MetaClassInfo();
            Assert.IsNotNull(metaData);
        }

        [Test]
        public void HasMetaInfoForEmptyClass()
        {
            dynamic metaData = MetaClassInfo();
            Assert.IsNotNull(metaData.EmptyClass);
        }
        [Test]
        public void HasNames()
        {
            var originalMetaData = TestMetaData.CreateEmptyClassMetaInfo();
            dynamic metaData = MetaClassInfo(originalMetaData);
            Assert.AreEqual("EmptyClass", metaData.EmptyClass.ClassName);
            Assert.AreEqual(originalMetaData.Single().TypeName.FullName, metaData.EmptyClass.ClassFullName);
        }
        [Test]
        public void HasFields()
        {
            var originalMetaData = TestMetaData.CreateSingleFieldClass();
            dynamic metaData = MetaClassInfo(originalMetaData);
            object meta2 = metaData;
            Console.Out.WriteLine(meta2);
            Assert.IsNotNull(metaData.SingleField);
            Assert.IsNotNull(metaData.SingleField.data);
        }
        [Test]
        public void HasIndexingState()
        {
            var originalMetaData = TestMetaData.CreateSingleFieldClass();
            dynamic metaData = MetaClassInfo(originalMetaData);
            Assert.AreEqual("data (Index: Unknown)",metaData.SingleField.data);
        }
        [Test]
        public void HasMetaDataForConflictingTypes()
        {
            var originalMetaData = TestMetaData.CreateNameConflicMetaInfo();
            dynamic metaData = MetaClassInfo(originalMetaData);
            Assert.NotNull(metaData.ANamespace.EmptyClass);
            Assert.NotNull(metaData.ANamespace.OtherNamespace.EmptyClass);
        }

        private object MetaClassInfo()
        {
            return MetaClassInfo(TestMetaData.CreateEmptyClassMetaInfo());
        }

        private object MetaClassInfo(IEnumerable<ITypeDescription> originalMetaData)
        {
            object dataContext = GetDataContext(originalMetaData);
            return GetMetaDataProperty(dataContext).GetValue(null, null);
        }

        private PropertyInfo GetMetaDataProperty(object dataContext)
        {
            return dataContext.GetType().GetProperty("MetaData");
        }


        private dynamic GetDataContext(IEnumerable<ITypeDescription> metaData)
        {
            var dataContextType = NewTestInstance(metaData).DataContext;
            return dataContextType.GetConstructors().Single().Invoke(new object[0]);
        }
    }
}