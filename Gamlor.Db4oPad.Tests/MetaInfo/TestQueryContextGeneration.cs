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
    public class TestQueryContextGeneration : TypeGenerationBase
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
        public void HasNewOperator()
        {
            var theProperty = SingleTypeQueryContext().GetProperty("EmptyClass");
            Assert.AreEqual(typeof(ExtendedQueryable<>),theProperty.PropertyType.GetGenericTypeDefinition());
        }
        [Test]
        public void HasStoreMethod()
        {
            TestUtils.WithTestContext(
                () =>
                {
                    var queryClass = SingleTypeQueryContext();
                    dynamic propertyForEmptyClass = queryClass.GetProperty("EmptyClass").GetValue(null,null);
                    var newInstance = propertyForEmptyClass.New();
                    queryClass.GetMethod("Store",
                        BindingFlags.Static | BindingFlags.Public
                        | BindingFlags.FlattenHierarchy).Invoke(null, new[] {newInstance});
                    Assert.AreEqual(1, Enumerable.Count(propertyForEmptyClass));
                });
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
                    var metaInfo = TypeWithGenericList();
                    var infos = NewTestInstance(metaInfo);
                    var properties = AllPropertiesExceptMetaData(infos);
                    Assert.AreEqual(1, properties.Count());
        }
        [Test]
        public void QueryForGenericType()
        {
                        var metaInfo = GenericType();
                        var infos = NewTestInstance(metaInfo);
                        var property = infos.DataContext.GetProperty("SingleField_1_String");
                    Assert.NotNull(property);
        }
        [Test]
        public void NoContextForArrayTypes()
        {
                    var metaInfo = TestMetaData.CreateClassWithArrayField();
                    var infos = NewTestInstance(metaInfo);
                    var properties = AllPropertiesExceptMetaData(infos);
                    Assert.AreEqual(1,properties.Count());
        }
        [Test]
        public void AddsConflictingNamesToNamespaces()
        {
            var typeInfos = TestMetaData.CreateNameConflicMetaInfo();
            var result = CodeGenerator.Create(typeInfos, TestUtils.NewName());
            dynamic theNamespace = result.DataContext.GetProperty(TestMetaData.Namespace).GetValue(null,null);
            TestUtils.WithTestContext(
                () =>
                    {
                        Assert.NotNull(theNamespace.EmptyClass);
                        Assert.NotNull(theNamespace.OtherNamespace);
                        Assert.NotNull(theNamespace.OtherNamespace.EmptyClass);
                    });
        }

        private IEnumerable<PropertyInfo> AllPropertiesExceptMetaData(CodeGenerationResult infos)
        {
            return (from p in infos.DataContext.GetProperties()
                    where p.Name!="MetaData"
                    select p);
        }


        private Type SingleTypeQueryContext()
        {
            var metaInfo = TestMetaData.CreateEmptyClassMetaInfo();
            var infos = NewTestInstance(metaInfo);
            return infos.DataContext;
        }
    }
}