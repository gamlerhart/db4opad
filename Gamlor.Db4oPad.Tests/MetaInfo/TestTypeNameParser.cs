using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.MetaInfo;
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
            Assert.AreEqual(1, result);
        }
        [Test]
        public void Identifier()
        {

            var result = TypeNameParser.Identifier.Parse(TypeName);
            Assert.AreEqual(TypeName, result);
        }
        [Test]
        public void AssemblyName()
        {
            var input = ", Assembly";

            var result = TypeNameParser.AssemblyName.Parse(input);
            Assert.AreEqual(AsseblyName, result);
        }

        [Test]
        public void GenericArgument()
        {

            var result = TypeNameParser.GenericArgument.Parse(GenericArg);
            Assert.AreEqual(TypeName, result.Name);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
        }
        [Test]
        public void GenericArgumentWithFollower()
        {
            var input = GenericArg + ", ";

            var result = TypeNameParser.GenericArgumentWithFollower.Parse(input);
            Assert.AreEqual(TypeName, result.Name);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
        }
        [Test]
        public void GenericArgListOneArg()
        {
            var input = string.Format("`1[{0}]", GenericArg);

            var result = TypeNameParser.GenericArgumentList.Parse(input);
            ValidateGenericArgs(result);
        }
        [Test]
        public void GenericArgListTwoArg()
        {
            var input = string.Format("`2[{0}, {0}]", GenericArg);

            var result = TypeNameParser.GenericArgumentList.Parse(input);
            ValidateGenericArgs(result);
        }

        [Test]
        public void SimpleType()
        {

            var result = TypeNameParser.TypeDefinition.Parse(SimleType);
            Assert.AreEqual(TypeName, result.Name);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
        }

        [Test]
        public void GenericTypeOneArg()
        {
            var input = string.Format("Namespace.List`1[{0}], {1}", GenericArg, AsseblyName);

            var result = TypeNameParser.TypeDefinition.Parse(input);
            Assert.AreEqual("Namespace.List", result.Name);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
            ValidateGenericArgs(result.GenericArguments);
        }
        [Test]
        public void GenericTypeTwoArg()
        {
            var input = string.Format("Namespace.Map`2[{0}, {0}], {1}", GenericArg, AsseblyName);

            var result = TypeNameParser.TypeDefinition.Parse(input);
            Assert.AreEqual("Namespace.Map", result.Name);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
            ValidateGenericArgs(result.GenericArguments);
        }
        [Test]
        public void GenericNested()
        {
            var input = string.Format("Namespace.Map`2[{0}, [Namespace.List`1[{0}], {1}]], {1}", GenericArg, AsseblyName);

            var result = TypeNameParser.TypeDefinition.Parse(input);
            Assert.AreEqual("Namespace.Map", result.Name);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
            ValidateGenericArgs(result.GenericArguments.Take(1));
            ValidateGenericArgs(result.GenericArguments.Last().GenericArguments);
        }


        [Test]
        public void ParseGenericNested()
        {
            var input = string.Format("Namespace.Map`2[{0}, [Namespace.List`1[{0}], {1}]], {1}", GenericArg, AsseblyName);

            var result = TypeNameParser.ParseString(input);
            Assert.AreEqual("Namespace.Map", result.Name);
            Assert.AreEqual(AsseblyName, result.AssemblyName);
            ValidateGenericArgs(result.GenericArguments.Take(1));
            ValidateGenericArgs(result.GenericArguments.Last().GenericArguments);
        }

        [Test]
        public void MultiNestedGeneric()
        {
            var input = "Gamlor.Db4oPad.Tests.TypeGeneration.Generic`2[[System.String, mscorlib], [System.Collections.Generic.List`1[[System.String, mscorlib]], mscorlib]], Gamlor.Db4oPad.Tests";


            var result = TypeNameParser.ParseString(input);
            Assert.AreEqual("Gamlor.Db4oPad.Tests.TypeGeneration.Generic", result.Name);
            Assert.AreEqual("Gamlor.Db4oPad.Tests", result.AssemblyName);
        }


        private void ValidateGenericArgs(IEnumerable<TypeName> result)
        {
            foreach (var type in result)
            {
                Assert.AreEqual(TypeName, type.Name);
                Assert.AreEqual(AsseblyName, type.AssemblyName);
            }
        }
    }

}