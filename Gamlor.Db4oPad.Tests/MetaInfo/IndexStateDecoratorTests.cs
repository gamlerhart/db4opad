using System;
using System.Linq;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using Gamlor.Db4oPad.Utils;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class IndexStateDecoratorTests
    {
        [Test]
        public void FieldsUseGivenIndexState()
        {
            var original = KnownType.Create(typeof(ClassWithFields));
            var withIndexInfo = IndexStateDecorator.Decorate(original,AllIndexed);

            Assert.AreEqual(IndexingState.Indexed, withIndexInfo.Fields.Single().IndexingState);
        }
        [Test]
        public void AffectsAlsoFieldsOfParent()
        {
            var orginalBase = TestMetaData.CreateSingleFieldClass();
            var originalSubClass = SimpleClassDescription.Create(TestMetaData.SingleFieldType(),
                                                                 Maybe.From(orginalBase.First()),
                                                                 (t) => new SimpleFieldDescription[0]);
            var withIndexInfo = IndexStateDecorator.Decorate(originalSubClass, AllIndexed);

            Assert.AreEqual(IndexingState.Indexed, withIndexInfo.BaseClass.Value.Fields.Single().IndexingState);
        }
        [Test]
        public void EqualityOfDecoratedTypes()
        {
            var original = KnownType.Create(typeof(ClassWithFields));
            var t1 = IndexStateDecorator.Decorate(original, AllIndexed);
            var t2 = IndexStateDecorator.Decorate(original, AllIndexed);

            HashCodeAsserts.AssertEquals(t1,t2);
        }
        [Test]
        public void EqualityOfDecoratedTypesBases()
        {
            var orginalBase = TestMetaData.CreateSingleFieldClass();
            var originalSubClass = SimpleClassDescription.Create(TestMetaData.SingleFieldType(),
                                                                 Maybe.From(orginalBase.First()),
                                                                 (t) => new SimpleFieldDescription[0]);
            var t1 = IndexStateDecorator.Decorate(originalSubClass, AllIndexed);
            var t2 = IndexStateDecorator.Decorate(originalSubClass, AllIndexed);

            HashCodeAsserts.AssertEquals(t1.BaseClass, t2.BaseClass);
        }
        [Test]
        public void DecorateTypesOfComplexFields()
        {
            var complexType = TestMetaData.CreateEmptyClassMetaInfo();
            var original = TestMetaData.CreateSingleFieldClass(complexType.First()).First();
            var decorated = IndexStateDecorator.Decorate(original, AllIndexed);
            var fieldType = decorated.Fields.Single().Type;
            Assert.IsTrue(fieldType is IndexStateDecorator);
        }
        [Test]
        public void CanDecorateRecursive()
        {
            var original = KnownType.Create(typeof(RecursiveClass));
            var decorated = IndexStateDecorator.Decorate(original, AllIndexed);
            var fieldType = decorated.Fields.Single().Type;
            Assert.IsTrue(fieldType is IndexStateDecorator);
        }
        [Test]
        public void DoNotDecorateRegularTypes()
        {
            var original = KnownType.Create(typeof(string));
            var decorated = IndexStateDecorator.Decorate(original, AllIndexed);
            Assert.AreSame(original,decorated);
        }
        [Test]
        public void DecoratedFieldHasRightType()
        {
            var original = KnownType.Create(typeof(ClassWithFields));
            var decorated = IndexStateDecorator.Decorate(original, AllIndexed);
            var fieldType = decorated.Fields.Single().Type;
            Assert.AreEqual(typeof(string).Name,fieldType.Name);
        }

        private IndexingState AllIndexed(TypeName typeName, SimpleFieldDescription fieldName)
        {
            return IndexingState.Indexed;
        }
    }
}