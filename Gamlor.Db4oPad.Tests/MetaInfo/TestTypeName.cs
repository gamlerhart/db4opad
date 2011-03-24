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
        public void ArrayEquals()
        {
            HashCodeAsserts.AssertEquals(CreateArrayType(), CreateArrayType());
        }
        [Test]
        public void NotEqual()
        {
            HashCodeAsserts.AssertNotEquals(CreateSimpleType(), CreateComplexType());
        }
        [Test]
        public void HasToString()
        {
            var name = CreateSimpleType();

            Assert.AreEqual("Type.Name",name.ToString());
        }
        [Test]
        public void ToStringPrintsName()
        {
            var theType = TypeName.Create("System.Int32", "mscorelib");
            Assert.AreEqual("System.Int32", theType.ToString());
        }

        private TypeName CreateComplexType()
        {
            var simple = TypeName.Create("Type.Name", "Assembly.Name");
            return TypeName.Create("Type.Map", "Assembly.Name", new[] { simple, simple });
        }
        private TypeName CreateArrayType()
        {
            return TypeName.Create("Type.Map", "Assembly.Name", new TypeName[0],1);
        }

        private TypeName CreateSimpleType()
        {
            return TypeName.Create("Type.Name", "Assembly.Name");
        }
    }

}