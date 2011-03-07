using Gamlor.Db4oPad.MetaInfo;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestTypeName
    {
        [Test]
        public void SimpleName()
        {
            var name = CreateSimpleType();
            Assert.AreEqual("Type.Name, Assembly.Name", name.FullName);
        }

        [Test]
        public void FullName()
        {
            var complex = CreateComplexType();

            Assert.AreEqual("Type.Map`2[[Type.Name, Assembly.Name], [Type.Name, Assembly.Name]], Assembly.Name",
                complex.FullName);
        }
        [Test]
        public void GenericName()
        {
            var complex = CreateComplexType();

            Assert.AreEqual("Type.Map`2[[Type.Name], [Type.Name]]",
                complex.NameWithGenerics);
        }
        [Test]
        public void SimpleEquals()
        {
            HashCodeAsserts.AssertEquals(CreateSimpleType(), CreateSimpleType());
        }
        [Test]
        public void ComplexEquals()
        {
            HashCodeAsserts.AssertEquals(CreateComplexType(), CreateComplexType());
        }
        [Test]
        public void NotEqual()
        {
            HashCodeAsserts.AssertNotEquals(CreateSimpleType(), CreateComplexType());
        }

        private TypeName CreateComplexType()
        {
            var simple = TypeName.Create("Type.Name", "Assembly.Name");
            return TypeName.Create("Type.Map", "Assembly.Name", new[] { simple, simple });
        }

        private TypeName CreateSimpleType()
        {
            return TypeName.Create("Type.Name", "Assembly.Name");
        }
    }

}