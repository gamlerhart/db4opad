using System.Collections.Generic;
using System.IO;
using System.Linq;
using Db4objects.Db4o.Internal;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Tests.TestTypes;
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
            var dynamicType = TestMetaData.CreateSingleFieldClass();
            var dynamicAssembly = CodeGenerator.Create(dynamicType, TestUtils.NewName());
            var typeToFind = dynamicAssembly.Types[dynamicType.Single(t=>t.IsBusinessEntity)];
            var found = TypeLoader.Create(new string[0])(TypeNameParser.ParseString(ReflectPlatform.FullyQualifiedName(typeToFind)));
            Assert.AreEqual(typeToFind, found.Value);
        }
        [Test]
        public void GetsGenericTypeFromCurrentAssembly()
        {
            var dynamicType = new []{TestMetaData.CreateGenericType()};
            var dynamicAssembly = CodeGenerator.Create(dynamicType, TestUtils.NewName());
            var typeToFind = dynamicAssembly.Types[dynamicType.Single(t => t.IsBusinessEntity)];
            var found = TypeLoader.Create(new string[0])(TypeNameParser.ParseString(ReflectPlatform.FullyQualifiedName(typeToFind)));
            Assert.AreEqual(typeToFind, found.Value);
        }
        [Test]
        public void LoadsFromAssemblies()
        {
            var found = TypeLoader.Create(new[] { @"..\..\Gamlor.Db4oPad.ExternalAssemblyForTests.dll" })
                (TypeNameParser.ParseString("Gamlor.Db4oPad.ExternalAssemblyForTests.AType, Gamlor.Db4oPad.ExternalAssemblyForTests"));
            Assert.IsNotNull(found.Value);
        }
        [Test]
        public void LoadGenericType()
        {
            var found = TypeLoader.Create(new[] { @"..\..\Gamlor.Db4oPad.ExternalAssemblyForTests.dll" })
                (TypeNameParser.ParseString("Gamlor.Db4oPad.ExternalAssemblyForTests.AGeneric`1[[System.Int32, mscorlib]], Gamlor.Db4oPad.ExternalAssemblyForTests"));
            Assert.IsTrue(found.HasValue);
            Assert.IsNotNull(found.Value.GetConstructors().Single().Invoke(new object[0]));
        }
        [Test]
        public void LoadMixedGenericType()
        {
            var genericArgument = TypeName.Create("DoesNotExist.ClassName", "DoesNotExist");
            var listName = TypeName.Create("System.Collections.Generic.List", "mscorlib", new[] {genericArgument});
            var found = TypeLoader.Create(new[] { @"..\..\Gamlor.Db4oPad.ExternalAssemblyForTests.dll" })(listName);
            Assert.IsFalse(found.HasValue);
        }
        [Test]
        public void DontCrashOnBogusAssemblies()
        {
            var found = TypeLoader.Create(new[] {Path.GetTempFileName() })(TypeName.Create("DoesNotExist.ClassName", "DoesNotExist"));
            Assert.IsFalse(found.HasValue);
        }
        [Test]
        public void DontCrashOnNonExistingFile()
        {
            var found = TypeLoader.Create(new[] { Path.GetRandomFileName() })(TypeName.Create("DoesNotExist.ClassName", "DoesNotExist"));
            Assert.IsFalse(found.HasValue);
        }
        [Test]
        public void CanLoadGenericTypeDefinition()
        {
            var genericArgument = TypeName.Create("DoesNotExist.ClassName", "DoesNotExist");
            var listName = TypeName.Create("System.Collections.Generic.List`1", "mscorlib", new[] { genericArgument });
            var type = TypeLoader.Create(new string[0])(listName.GetGenericTypeDefinition());
            Assert.AreEqual(typeof(List<>), type.Value);
        }
        
    }
}