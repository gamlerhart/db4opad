using Gamlor.Db4oPad.MetaInfo;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestTypeDescriptions
    {
        [Test]
        public void GenericName()
        {
            TypeName genericArg = TypeName.Create("System.Int32","mscorelib");
            TypeName theName = TypeName.Create("TheType", "TheAssembly", new[] { genericArg });
            var theType = SimpleClassDescription.Create(theName, f => new SimpleFieldDescription[0]);
            Assert.AreEqual("TheType_1",theType.Name);
        }
        [Test]
        public void ObjectIsRecursive()
        {
            var type = SystemType.Object;
            Assert.AreEqual(type,type.BaseClass);
        }
    }
}