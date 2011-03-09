using System;
using Gamlor.Db4oPad.Utils;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.Utils
{
    [TestFixture]
    public class TestDisposer
    {

        [Test]
        public void ExpectNoDisposeOnAdd()
        {
            var dsp1 = new AssertDisposable();
            var toTest = new Disposer();
            toTest.Add(dsp1);
            dsp1.AssertNotDisposed();
        }

        [Test]
        public void CanEmptyDispose()
        {
            new Disposer().Dispose();
        }

        [Test]
        public void ThrowOnDisposed()
        {
            var toTest = new Disposer();
            toTest.Dispose();
            Assert.Throws(typeof(ObjectDisposedException), toTest.CheckNotDisposed);
        }

        [Test]
        public void ThrowOnAdd()
        {
            var toTest = new Disposer();
            toTest.Dispose();
            Assert.Throws(typeof(ObjectDisposedException), () => toTest.Add(new AssertDisposable()));
        }

        [Test]
        public void ExpectDisposes()
        {
            var dsp1 = new AssertDisposable();
            var dsp2 = new AssertDisposable();
            var dsp3 = new AssertDisposable();
            var toTest = new Disposer();
            toTest.Add(dsp1);
            toTest.Add(dsp2);
            toTest.Add(dsp3);
            toTest.Dispose();
            dsp1.AssertDisposed();
            dsp2.AssertDisposed();
            dsp3.AssertDisposed();
        }
        [Test]
        public void AddDisposeClosure()
        {
            var ran = false;
            var toTest = new Disposer();
            toTest.AddAsDisposable(() => { ran = true; });
            toTest.Dispose();
            Assert.IsTrue(ran);
        }
        [Test]
        public void AddCasted()
        {
            var toCheck = new AssertDisposable();
            object toAdd = toCheck;
            var toTest = new Disposer();
            toTest.AddIfDisposable(toAdd);
            toTest.Dispose();
            toCheck.AssertDisposed();
        }
        [Test]
        public void CanAddNonDisposableCasted()
        {
            object toAdd = new object();
            var toTest = new Disposer();
            toTest.AddIfDisposable(toAdd);
            toTest.Dispose();
        }

        [Test]
        public void CannotDisposeTwiceAction()
        {
            var disposable = Disposer.AsDisposable(() => { });
            disposable.Dispose();
            Assert.Throws(typeof(ObjectDisposedException), disposable.Dispose);
        }

        [Test]
        public void AsDisposableCallsAction()
        {
            var wasCalled = false;
            var disposable = Disposer.AsDisposable(() => wasCalled = true);
            disposable.Dispose();
            Assert.IsTrue(wasCalled);
        }
        [Test]
        public void EventAsDisposable()
        {
            var eventObject = new EventObject();
            var toTest = new Disposer();
            toTest.Event<Action<string>>(e => Assert.Fail("Unexpected"),
                e => eventObject.AEvent += e,
                e => eventObject.AEvent -= e);
            toTest.Event<Action<string, string>>((f, s) => Assert.Fail("Unexpected"),
                e => eventObject.OtherEvent += e,
                e => eventObject.OtherEvent -= e);
            toTest.Event<Action<string, string, string>>((f, s, t) => Assert.Fail("Unexpected"),
                e => eventObject.ThirdEvent += e,
                e => eventObject.ThirdEvent -= e);
            toTest.Dispose();
            eventObject.Raise();
        }
        [Test]
        public void EventAsDisposableRegisters()
        {
            var eventObject = new EventObject();
            var callCount = 0;
            var toTest = new Disposer();
            toTest.Event<Action<string>>(e => callCount++,
                e => eventObject.AEvent += e,
                e => eventObject.AEvent -= e);
            toTest.Event<Action<string, string>>((f, s) => callCount++,
                e => eventObject.OtherEvent += e,
                e => eventObject.OtherEvent -= e);
            toTest.Event<Action<string, string, string>>((f, s, t) => callCount++,
                e => eventObject.ThirdEvent += e,
                e => eventObject.ThirdEvent -= e);
            eventObject.Raise();
            Assert.AreEqual(3, callCount);
        }


    }
    internal class EventObject
    {
        public event Action<string> AEvent;
        public event Action<string, string> OtherEvent;
        public event Action<string, string, string> ThirdEvent;

        public EventObject()
        {
            AEvent += (e) => { };
            OtherEvent += (f, s) => { };
            ThirdEvent += (f, s, t) => { };
        }

        public void Raise()
        {
            AEvent("first");
            OtherEvent("first", "second");
            ThirdEvent("first", "second", "third");
        }
    }
}