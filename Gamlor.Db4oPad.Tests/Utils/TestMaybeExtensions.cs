using System.Collections.Generic;
using Gamlor.Db4oPad.Utils;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.Utils
{
    [TestFixture]
    public class TestMaybeExtensions
    {
        [Test]
        public void GetExistingReferenceType()
        {
            const string expectedValue = "value";
            var dic = SingleEntry("key", expectedValue);
            var result = dic.TryGet("key");
            Assert.IsTrue(result.HasValue);
            Assert.AreSame(expectedValue, result.Value);
        }
        [Test]
        public void NonExistingReferenceType()
        {
            var dic = new Dictionary<string, string>();
            var result = dic.TryGet("key");
            Assert.IsFalse(result.HasValue);
        }
        [Test]
        public void GetExistingValueType()
        {
            var dic = SingleEntry("key", 42);
            var result = dic.TryGet("key");
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(42, result.Value);
        }
        [Test]
        public void NonExistingValueType()
        {
            var dic = new Dictionary<string, int>();
            var result = dic.TryGet("key");
            Assert.IsFalse(result.HasValue);
        }

        [Test]
        public void AsMaybeEmptyOnNull()
        {
            object obj = null;
            var asMaybe = obj.AsMaybe();
            Assert.IsFalse(asMaybe.HasValue);
        }
        [Test]
        public void AsMaybeValue()
        {
            object obj = "fun";
            var asMaybe = obj.AsMaybe();
            Assert.IsTrue(asMaybe.HasValue);
            Assert.AreEqual("fun", asMaybe.Value);
        }
        [Test]
        public void FirstMaybeEmpty()
        {
            IEnumerable<string> buncOfObjects = new string[0];
            var asMaybe = buncOfObjects.FirstMaybe();
            Assert.IsFalse(asMaybe.HasValue);
        }
        [Test]
        public void FirstMaybeValue()
        {
            IEnumerable<string> buncOfObjects = new[] { "value" };
            var asMaybe = buncOfObjects.FirstMaybe();
            Assert.IsTrue(asMaybe.HasValue);
            Assert.AreEqual("value", asMaybe.Value);
        }

        [Test]
        public void CanCast()
        {
            object obj = "fun";
            var asMaybe = obj.MaybeCast<string>();
            Assert.IsTrue(asMaybe.HasValue);
            Assert.AreEqual("fun", asMaybe.Value);
        }

        [Test]
        public void CanNotCast()
        {
            object obj = "42";
            var asMaybe = obj.MaybeCast<int>();
            Assert.IsFalse(asMaybe.HasValue);
        }

        private static IDictionary<TKey, TValue> SingleEntry<TKey, TValue>(TKey key, TValue value)
        {
            return new Dictionary<TKey, TValue>() { { key, value } };
        }
    }
}