using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Utils;
using NUnit.Framework;
using Sprache;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    [TestFixture]
    public class TestTypeNameParser
    {
        private const string TypeName = "Namespace.Type";
        private const string AsseblyName = "Assembly";
        private readonly static string SimleType = string.Format("{0}, {1}", TypeName, AsseblyName);
        private readonly static string GenericArg = string.Format("[{0}]", SimleType);

        [Test]
        public void ParseSeparator()
        {
            var input = ", ";

            var result = TypeNameParser.Seperator.Parse(input);
            Assert.AreEqual(',', result);
        }
        [Test]
        public void ParenthesisOpen()
        {
            var input = "[";

            var result = TypeNameParser.ParentherisOpen.Parse(input);
            Assert.AreEqual('[', result);
        }
        [Test]
        public void ParenthesisClose()
        {
            var input = "]";

            var result = TypeNameParser.ParentherisClose.Parse(input);
            Assert.AreEqual(']', result);
        }
        [Test]
        public void ParseGenericArgumentSize()
        {
            var input = "`1";

            var result = TypeNameParser.GenericIndicator.Parse(input);
            Assert.AreEqual(1, result.GenericCount);
        }
        [Test]
        public void Identifier()
        {

            var result = TypeNameParser.Identifier.Parse(TypeName);
            Assert.AreEqual(TypeName, result.String);
        }
        [Test]
        public void ArrayOfOne()
        {

            var result = TypeNameParser.AnArray.Parse("[]");
            Assert.AreEqual(1, result);
        }
        [Test]
        public void ArrayOfZero()
        {

            var result = TypeNameParser.AnArray.Parse("");
            Assert.AreEqual(0, result);
        }
        [Test]
        public void AssemblyName()
        {
            var input = ", Assembly";

            var result = TypeNameParser.AssemblyName.Parse(input);
            Assert.AreEqual(AsseblyName, result);
        }
        [Test]
        public void AssemblyNameCanContainsUnderline()
        {
            var input = ", Assembly_random";

            var result = TypeNameParser.AssemblyName.Parse(input);
            Assert.AreEqual("Assembly_random", result);
        }
        [Test]
        public void AssemblyProperty()
        {
            var input = ", Version=1.2.3.4";

            var result = TypeNameParser.AssemblyProperty.Parse(input);
            Assert.AreEqual("Version=1.2.3.4", result);
        }

        [Test]
        public void GenericArgument()
        {

            var result = TypeNameParser.GenericArgument.Parse(GenericArg);
            Assert.AreEqual(TypeName, result.NameAndNamespace);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
        }
        [Test]
        public void GenericArgumentWithFollower()
        {
            var input = GenericArg + ", ";

            var result = TypeNameParser.GenericArgumentWithFollower.Parse(input);
            Assert.AreEqual(TypeName, result.NameAndNamespace);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
        }
        [Test]
        public void GenericArgListOneArg()
        {
            var input = string.Format("[{0}]", GenericArg);

            var result = TypeNameParser.GenericArgumentList(1).Parse(input);
            ValidateGenericArgs(result);
        }
        [Test]
        public void GenericArgListTwoArg()
        {
            var input = string.Format("[{0}, {0}]", GenericArg);

            var result = TypeNameParser.GenericArgumentList(2).Parse(input);
            ValidateGenericArgs(result);
        }

        [Test]
        public void SimpleType()
        {

            var result = TypeNameParser.TypeDefinition.Parse(SimleType);
            Assert.AreEqual(TypeName, result.NameAndNamespace);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
        }

        [Test]
        public void GenericTypeOneArg()
        {
            var input = string.Format("Namespace.List`1[{0}], {1}", GenericArg, AsseblyName);

            var result = TypeNameParser.TypeDefinition.Parse(input);
            Assert.AreEqual("Namespace.List`1", result.NameAndNamespace);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
            ValidateGenericArgs(result.GenericArguments);
        }
        [Test]
        public void GenericTypeTwoArg()
        {
            var input = string.Format("Namespace.Map`2[{0}, {0}], {1}", GenericArg, AsseblyName);

            var result = TypeNameParser.TypeDefinition.Parse(input);
            Assert.AreEqual("Namespace.Map`2", result.NameAndNamespace);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
            ValidateGenericArgs(result.GenericArguments);
        }
        [Test]
        public void GenericNested()
        {
            var input = string.Format("Namespace.Map`2[{0}, [Namespace.List`1[{0}], {1}]], {1}", GenericArg, AsseblyName);

            var result = TypeNameParser.TypeDefinition.Parse(input);
            Assert.AreEqual("Namespace.Map`2", result.NameAndNamespace);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
            ValidateGenericArgs(result.GenericArguments.Take(1));
            ValidateGenericArgs(result.GenericArguments.Last().Value.GenericArguments);
        }


        [Test]
        public void ParseGenericNested()
        {
            var input = string.Format("Namespace.Map`2[{0}, [Namespace.List`1[{0}], {1}]], {1}", GenericArg, AsseblyName);

            var result = TypeNameParser.ParseString(input);
            Assert.AreEqual("Namespace.Map`2", result.NameAndNamespace);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
            ValidateGenericArgs(result.GenericArguments.Take(1));
            ValidateGenericArgs(result.GenericArguments.Last().Value.GenericArguments);
        }

        [Test]
        public void MultiNestedGeneric()
        {
            var input = "Gamlor.Db4oPad.Tests.TypeGeneration.Generic`2[[System.String, mscorlib], [System.Collections.Generic.List`1[[System.String, mscorlib]], mscorlib]], Gamlor.Db4oPad.Tests";


            var result = TypeNameParser.ParseString(input);
            Assert.AreEqual("Gamlor.Db4oPad.Tests.TypeGeneration.Generic`2", result.NameAndNamespace);
            Assert.AreEqual("Gamlor.Db4oPad.Tests", result.AssemblyName);
        }

        [Test]
        public void MultiGenericNested()
        {
            var input = string.Format("Namespace.MainClass`1+NestedClass`2[{0}, {0}, {0}], {1}", GenericArg, AsseblyName);
            
            var result = TypeNameParser.ParseString(input);
            Assert.AreEqual("Namespace.MainClass`1+NestedClass`2", result.NameAndNamespace);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
            ValidateGenericArgs(result.GenericArguments.Take(1));
            ValidateGenericArgs(result.GenericArguments.Skip(1).Take(1));
            ValidateGenericArgs(result.GenericArguments.Last().Value.GenericArguments);
        }

        [Test]
        public void StringArray()
        {
            var input = "System.String[], mscorlib";


            var result = TypeNameParser.ParseString(input);
            Assert.AreEqual("System.String[]", result.NameAndNamespace);
            Assert.AreEqual("mscorlib", result.AssemblyName);
            Assert.AreEqual("System.String[], mscorlib", result.FullName);
            Assert.AreEqual(1, result.OrderOfArray);
            Assert.AreEqual("System.String, mscorlib", result.ArrayOf.Value.FullName);
        }

        [Test]
        public void NestedClasses()
        {
            var input = "Db4oPad.TestDBs.NestedClasses+ChildClass, Db4oPad.TestDBs";
            var result = TypeNameParser.ParseString(input);
            Assert.AreEqual("Db4oPad.TestDBs.NestedClasses+ChildClass", result.NameAndNamespace);
            Assert.AreEqual("Db4oPad.TestDBs", result.AssemblyName);
            Assert.AreEqual(input, result.FullName);
            Assert.AreEqual("ChildClass", result.Name);
        }


        private void ValidateGenericArgs(IEnumerable<TypeName> result)
        {
            foreach (var type in result)
            {
                Assert.AreEqual(TypeName, type.NameAndNamespace);
                Assert.AreEqual(AsseblyName, type.AssemblyName);
            }
        }

        private void ValidateGenericArgs(IEnumerable<Maybe<TypeName>> result)
        {
            ValidateGenericArgs(result.Select(t => t.Value));
        }
    }

}