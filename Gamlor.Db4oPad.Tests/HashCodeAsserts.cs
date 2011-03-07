using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    static class HashCodeAsserts
    {
        /// <summary>
        /// Basically check if the <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>
        /// work properly together
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        public static void AssertEquals(object o1, object o2)
        {
            Assert.AreEqual(o1.GetHashCode(), o2.GetHashCode(), "Hash have to be equal");
            Assert.IsTrue(o1.Equals(o2));
            Assert.IsTrue(o2.Equals(o1));
            Assert.IsTrue(o2.Equals(o2));
            Assert.IsTrue(o1.Equals(o1));
        }

        /// <summary>
        /// Basically check if the <see cref="object.Equals(object)"/> works for non-equal-checks
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        public static void AssertNotEquals(object o1, object o2)
        {
            Assert.AreNotEqual(o1, o2);
            Assert.AreNotEqual(o2, o1);
        }
    }

}