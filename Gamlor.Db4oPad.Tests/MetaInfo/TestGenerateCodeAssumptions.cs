using System;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestGenerateCodeAssumptions
    {
        [Test]
        public void ArrayOfRegularType()
        {
            var type = typeof (RegularType).MakeArrayType();
            Assert.IsTrue(type.AssemblyQualifiedName.Contains("[]"));
        }
        [Test]
        public void ArrayOfGeneratedType()
        {
            var simpleType = TestMetaData.CreateEmptyClassMetaInfo();
            var code = CodeGenerator.Create(simpleType, TestUtils.NewName());
            var generatedType = code.Types[simpleType.Single()];
            var type = generatedType.MakeArrayType();
            Assert.IsTrue(type.AssemblyQualifiedName.Contains("[]"));
        }



        class RegularType
        {
            
        }
    }
}