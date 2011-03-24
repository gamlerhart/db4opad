using Gamlor.Db4oPad.MetaInfo;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestTypeLoader
    {
        [Test]
        public void DelegatesToNativeLoader()
        {
            var typeName = TypeName.Create("System.String", "mscorlib");
            var type = TypeLoader.Create(new string[0])(typeName);
            Assert.AreEqual(typeof(string),type.Value);
        }
        [Test]
        public void ReturnsEmptyResultOnUnknownType()
        {
            var typeName = TypeName.Create("DoesNotExist.ClassName", "DoesNotExist");
            var type = TypeLoader.Create(new string[0])(typeName);
            Assert.IsFalse(type.HasValue);
        }
        [Test]
        public void GetsTypeFromCurrentAssembly()
        {
            Assert.Fail("Todo");
        }
        
    }
}