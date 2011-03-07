using System;
using Gamlor.Db4oPad.Utils;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.Utils
{
    [TestFixture]
    public class TestMaybe
    {
        private readonly Maybe<string> testInstance;

        [Test]
        public void DefaultIsEmpty()
        {
            Assert.IsFalse(testInstance.HasValue);
            Assert.AreEqual(Maybe<string>.Empty, testInstance);
        }
        [Test]
        public void CannotAssignNull()
        {
            Assert.Throws(typeof(ArgumentNullException),
                () =>
                {
                    Maybe<string> fun = null;
                });
        }
        [Test]
        public void IsNotSet()
        {
            var test = Maybe<int>.Empty;
            Assert.IsFalse(test.HasValue);
        }
        [Test]
        public void DefaultGet()
        {
            var test = Maybe<string>.Empty;
            Assert.AreEqual("no-value", test.GetValue("no-value"));
        }
        [Test]
        public void ContainsValue()
        {
            var test = Maybe.From(1);
            Assert.IsTrue(test.HasValue);
            Assert.AreEqual(1, test.Value);
        }
        [Test]
        public void Equals()
        {
            Maybe<string> m1 = "fun";
            Maybe<string> m2 = "fun";
            Assert.AreEqual(m1, m2);
        }
        [Test]
        public void EqualsWithOriginal()
        {
            Maybe<string> m1 = "fun";
            Assert.AreEqual(m1, "fun");
        }
        [Test]
        public void NullIsFalse()
        {
            Assert.Throws(typeof(ArgumentNullException), () => Maybe.From<string>(null));
        }

        [Test]
        public void CannotAccessNullValue()
        {
            var test = Maybe<int>.Empty;
            Assert.Throws(typeof(InvalidOperationException), () => test.Value.ToString());
        }

        [Test]
        public void CanConvert()
        {
            var test = Maybe.From(1);
            Assert.AreEqual("1", test.Convert(c => c.ToString()).Value);
        }
        [Test]
        public void CanConvertNotSet()
        {
            var test = Maybe<int>.Empty;
            Assert.IsFalse(test.Convert(c => c.ToString()).HasValue);
        }
        [Test]
        public void ApplyIsCalledWithValue()
        {
            var wasCalled = false;
            Maybe.From(true).Apply(v => wasCalled = v);
            Assert.IsTrue(wasCalled);
        }
        [Test]
        public void ApplyIsNotCalledWithoutValue()
        {
            var wasCalled = false;
            Maybe<bool>.Empty.Apply(v => wasCalled = v);
            Assert.IsFalse(wasCalled);
        }
        [Test]
        public void HandleIsCalledWithoutValue()
        {
            var wasCalled = false;
            Maybe<bool>.Empty.Handle(() => wasCalled = true);
            Assert.IsTrue(wasCalled);
        }
        [Test]
        public void HandleIsNotCalledWithValue()
        {
            var wasCalled = false;
            Maybe.From(true).Handle(() => wasCalled = true);
            Assert.IsFalse(wasCalled);
        }
        [Test]
        public void CanChainApplyHandle()
        {
            var wasCalledApply = false;
            var wasCalledHandle = false;
            Maybe.From(true)
                .Handle(() => wasCalledHandle = true)
                .Apply(v => wasCalledApply = v);
            Assert.IsTrue(wasCalledApply);
            Assert.IsFalse(wasCalledHandle);
        }
        [Test]
        public void ConvertChainValue()
        {
            var value = Maybe.From("value")
                .Convert(s => s.ToUpper())
                .GetValue("default");
            Assert.AreEqual("VALUE", value);
        }
        [Test]
        public void ConvertChainDefault()
        {
            var value = Maybe<string>.Empty
                .Convert(s => s.ToUpper())
                .GetValue("default");
            Assert.AreEqual("default", value);
        }


        [Test]
        public void SuccessfulOutParameterAsMaybe()
        {
            var value = Maybe.OutToMaybe<object>(SuccessFulOut);
            Assert.AreEqual("default", value.Value);
        }
        [Test]
        public void SuccessfulOutParamWithLocalVariables()
        {
            var value = Maybe.OutToMaybe((out object outP) => SuccessFulOut("arg", out outP));
            Assert.AreEqual("default", value.Value);
        }
        [Test]
        public void UnsuccessfulOutParameterAsMaybe()
        {
            var value = Maybe.OutToMaybe<object>(UnsuccessFulOut);
            Assert.IsFalse(value.HasValue);
        }

        private bool SuccessFulOut(out object outData)
        {
            outData = "default";
            return true;
        }
        private bool SuccessFulOut(object argument, out object outData)
        {
            outData = "default";
            return true;
        }

        private bool UnsuccessFulOut(out object outData)
        {
            outData = null;
            return false;
        }
    }
}