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
        private static MethodInfo StaticQueryCall(Type forType)
        {
            return typeof(CurrentContext).GetMethod("Query").MakeGenericMethod(forType);
        }

        private Type SingleTypeQueryContext()
        {
            var metaInfo = CreateEmptyClassMetaInfo();
            var infos = NewTestInstance(metaInfo);
            return infos.DataContext;
        }
    }
}