using System;
using LINQPad;
using LINQPad.User;
using NUnit.Framework;
using System.Linq;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class TestMemberProviders
    {
        [Test]
        public void IsUsedInDriver()
        {
            var toTest = new Db4oDriver();
            var showInfo = toTest.GetCustomDisplayMemberProvider(new FieldsAndProperties());

            AssertInfo(showInfo);
        }
        [Test]
        public void NullGiveNullResult()
        {
            var toTest = new Db4oDriver();
            var showInfo = toTest.GetCustomDisplayMemberProvider(null);
            Assert.IsNull(showInfo);

        }
        [Test]
        public void OnlyShowProperties()
        {
            var showInfo = MemberProvider.Create(new FieldsAndProperties());

            AssertInfo(showInfo.Value);
        }
        [Test]
        public void ExceptionIsNotVisualized()
        {
            var showInfo = MemberProvider.Create(new ArgumentException("test"));

            Assert.IsFalse(showInfo.HasValue);
        }
        [Test]
        public void OnlyApplyToGeneratedTypes()
        {
            var showInfo = MemberProvider.Create(new ArgumentException("test"));

            Assert.IsFalse(showInfo.HasValue);
        }

        private void AssertInfo(ICustomMemberProvider showInfo)
        {
            Assert.AreEqual(3,showInfo.GetNames().Count());
            Assert.AreEqual(3,showInfo.GetTypes().Count());
            Assert.AreEqual(3,showInfo.GetValues().Count());


            Assert.IsTrue(new[]{"AutoProperty","Field","OtherField"}.SequenceEqual(showInfo.GetNames()));
            Assert.IsTrue(new[]{typeof(string),typeof(int),typeof(long)}.SequenceEqual(showInfo.GetTypes()));
            Assert.IsTrue(new object[] { null,1, 2L }.SequenceEqual(showInfo.GetValues()));
        }


    }
}
namespace LINQPad.User
{
    class FieldsAndProperties
    {
        public int field = 1;
        public int Field { get { return field; } }
        public long otherField = 2L;
        public long OtherField { get { return otherField; } }
        public string AutoProperty { get; set; }
    }
    
}