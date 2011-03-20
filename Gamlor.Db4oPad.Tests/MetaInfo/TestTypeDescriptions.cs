using System;
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
        public void ThrowIfArrayType()
        {
            TypeName theName = TypeName.Create("System.Int32", "mscorelib",new TypeName[0],1);
            Assert.Throws<ArgumentException>(()=>SimpleClassDescription.Create(theName, f => new SimpleFieldDescription[0]));
            
        }
        [Test]
        public void ArrayType()
        {
            TypeName theName = TypeName.Create("System.Int32", "mscorelib", new TypeName[0], 0);
            var innerType = SimpleClassDescription.Create(theName, f => new SimpleFieldDescription[0]);
            var arrayType = ArrayDescription.Create(innerType, 1);
            Assert.IsTrue(arrayType.IsArray);
            Assert.AreEqual(innerType,arrayType.ArrayOf.Value);

        }
        [Test]
        public void ObjectIsRecursive()
        {
            var type = SystemType.Object;
            Assert.AreEqual(type,type.BaseClass);
        }
    }
}