using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    class TestQueryContextGeneration : TypeGenerationBase
    {
        [Test]
        public void HasQueryContext()
        {
            var queryClass = SingleTypeQueryContext();
            Assert.NotNull(queryClass);
        }
        [Test]
        public void HasQueryProperty()
        {
            var queryClass = SingleTypeQueryContext();
            var property = queryClass.GetProperty("EmptyClass");
            Assert.NotNull(property);
        }
        [Test]
        public void CanQuery()
        {
            TestUtils.WithTestContext(
                () =>
                            {
                                var queryClass = SingleTypeQueryContext();
                                var property = queryClass.GetProperty("EmptyClass");
                                var query = property.GetValue(null, new object[0]);
                                Assert.NotNull(query);
                            });
        }
        [Test]
        public void OnlyHasQueriesForComplexTypes()
        {
            TestUtils.WithTestContext(
                () =>
                {
                    var metaInfo = TypeWithGenericList();
                    var infos = NewTestInstance(metaInfo);
                    var properties = infos.DataContext.GetProperties();
                    Assert.AreEqual(1, properties.Length);
                });
        }
        [Test]
        public void QueryForGenericType()
        {
            TestUtils.WithTestContext(
                () =>
                    {
                        var metaInfo = GenericType();
                        var infos = NewTestInstance(metaInfo);
                        var property = infos.DataContext.GetProperty("SingleField_1");
                    Assert.NotNull(property);
                });
        }

        private Type SingleTypeQueryContext()
        {
            var metaInfo = TestMetaData.CreateEmptyClassMetaInfo();
            var infos = NewTestInstance(metaInfo);
            return infos.DataContext;
        }
    }
}