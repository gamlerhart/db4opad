using System;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class TestExtendedQueryable
    {
        [Test]
        public void WorksLikeTheOriginal()
        {
            var original = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9}.AsQueryable();
            var wrapped = ExtendedQueryable.Create(original);

            Assert.IsTrue(original.SequenceEqual(wrapped));
            Assert.IsTrue(original.SequenceEqual(wrapped.Select(n => n)));
            Assert.IsTrue(original.Where(n => n > 5).SequenceEqual(wrapped.Where(n => n > 5)));
            Assert.AreEqual(original.ElementType,wrapped.ElementType);
            Assert.AreEqual(original.Expression, wrapped.Expression);
        }
        [Test]
        public void CanCreateNewInstance()
        {
            var original = new[] { 1 }.AsQueryable();
            var wrapped = ExtendedQueryable.Create(original);

            Assert.AreEqual(0, wrapped.New());
        }
        [Test]
        public void CanCreateNewInstanceOfComplexType()
        {
            var wrapped = QueryableOfComplexType();

            Assert.AreEqual(42,wrapped.New().Number);
        }

        [Test]
        public void CanInstatiateWithArguments()
        {
            var wrapped = QueryableOfComplexType();

            Assert.AreEqual(55, wrapped.New(55).Number);
        }
        [Test]
        public void CanInstatiateWithMultipleArguments()
        {
            var wrapped = QueryableOfComplexType();

            Assert.AreEqual(55, wrapped.New(55,"hello").Number);
            Assert.AreEqual("hello", wrapped.New(55, "hello").Data);
        }
        [Test]
        public void NullArgumentThrowsAppropriateException()
        {
            var wrapped = QueryableOfComplexType();

            Assert.Throws<ArgumentException>(() => wrapped.New(55, null));
        }

        private ExtendedQueryable<MoreComplexType> QueryableOfComplexType()
        {
            var original = new MoreComplexType[0].AsQueryable();
            return ExtendedQueryable.Create(original);
        }

        // Test class, which is partially used by reflection
        // Therefore we can deactivate the warnings here
        // ReSharper disable UnusedMember.Local
        class MoreComplexType
        {
            public MoreComplexType()
            {
                Number = 42;

            }
            public MoreComplexType(int number)
            {
                Number = number;
            }
            public MoreComplexType(int number, string data)
            {
                Data = data;
                Number = number;
            }

            public int Number { get; set; }
            public string Data { get; set; }

        }
        // ReSharper restore UnusedMember.Local
        
    }
}