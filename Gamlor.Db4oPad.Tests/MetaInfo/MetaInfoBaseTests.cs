using System.Linq;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class MetaInfoBaseTests
    {
        [Test]
        public void ProperToString()
        {
            var metaInfo = TestMetaData.CreateEmptyClassMetaInfo();
            var typeDescription = metaInfo.Single();
            var toTest = new TestEntity(typeDescription);
            Assert.IsTrue(toTest.ToString().Contains(typeDescription.TypeName.FullName));
        }


        class TestEntity : MetaInfoBase
        {
            public TestEntity(ITypeDescription description) : base(description)
            {
            }
        }
        
    }
}