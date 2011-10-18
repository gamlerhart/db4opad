using System;
using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.Utils;
using Sprache;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class IdentWithGenericCount
    {
        public IdentWithGenericCount(string name, int genericCount)
        {
            String = name;
            GenericCount = genericCount;
        }

        public string String { get; set; }
        public int GenericCount { get; set; }
    }

    public static class TypeNameParser
    {
        internal static readonly Parser<IdentWithGenericCount> GenericIndicator = from sep in Parse.Char('`').Select(x => new IdentWithGenericCount(x.ToString(), 0))
                                                                                  from number in Parse.Number.Select(x => new IdentWithGenericCount(x.ToString(), int.Parse(x)))
                                                                                  select new IdentWithGenericCount(sep.String + number.String, sep.GenericCount + number.GenericCount);

        internal static readonly Parser<IdentWithGenericCount> Identifier = from id in (Parse.LetterOrDigit.Select(x => new IdentWithGenericCount(x.ToString(), 0)))
                                                        .Or<IdentWithGenericCount>(Parse.Char('+').Select(x => new IdentWithGenericCount(x.ToString(), 0)))
                                                        .Or<IdentWithGenericCount>(Parse.Char('.').Select(x => new IdentWithGenericCount(x.ToString(), 0)))
                                                        .Or<IdentWithGenericCount>(Parse.Char('_').Select(x => new IdentWithGenericCount(x.ToString(), 0)))
                                                        .Or<IdentWithGenericCount>(GenericIndicator)
                                                        .Many()
                                                        let fullString = id.Select(x => x.String).ToArray()
                                                        let totalGenericCount = id.Select(x => x.GenericCount).Sum()
                                                        select new IdentWithGenericCount(string.Join("", fullString), totalGenericCount);


        internal static readonly Parser<char> Seperator =
                                                 from space in Parse.Char(' ').Many()
                                                 from sep in Parse.Char(',')
                                                 from space2 in Parse.Char(' ').Many()
                                                 select sep;

        internal static readonly Parser<string> AssemblyName = from sep in Seperator
                                                      from id in Identifier.Select(x => x.String)
                                                      select id;

        internal static readonly Parser<string> AssemblyProperty = from s in Seperator
                                                                 from prop in Parse.LetterOrDigit
                                                                     .Or(Parse.Char('='))
                                                                     .Or(Parse.Char('.')).Many()
                                                                 select new string(prop.ToArray());

        internal static readonly Parser<char> ParentherisOpen = from sep in Parse.Char('[')
                                                       select sep;

        internal static readonly Parser<char> ParentherisClose = from sep in Parse.Char(']')
                                                        select sep;


        private static readonly Parser<int> OneArrayIndicator = from o in ParentherisOpen
                                            from c in ParentherisClose
                                            select 1;

        internal static readonly Parser<int> AnArray = from arrays in OneArrayIndicator.Many()
                                              select arrays.Count();

        internal static readonly Parser<TypeName> TypeDefinition = from typeName in Identifier
// ReSharper disable StaticFieldInitializersReferesToFieldBelow
                                                          //  Because the Parse.Ref() protects us from this issue
                                                          from argList in Parse.Ref(() => GenericArgumentList(typeName.GenericCount)).Many()
                                                          // ReSharper restore StaticFieldInitializersReferesToFieldBelow
                                                          from array in AnArray
                                                          from assemblyName in AssemblyName
                                                          from assemblyProps in AssemblyProperty.Many()
                                                          select
                                                              CreateType(typeName.String, assemblyName,array,
                                                                         argList.SingleOrDefault());


        internal static readonly Parser<TypeName> GenericArgument = from o in ParentherisOpen
                                                           from type in TypeDefinition
                                                           from c in ParentherisClose
                                                           select type;


        internal static readonly Parser<TypeName> GenericArgumentWithFollower = from type in GenericArgument
                                                                       from s in Seperator
                                                                       select type;

        internal static readonly Func<int, Parser<IEnumerable<TypeName>>> GenericArgumentList = expectedLength =>
                                                                            from o in ParentherisOpen
                                                                            from args in
                                                                                GenericArgumentWithFollower.Many()
                                                                            from lastArg in GenericArgument
                                                                            from c in ParentherisClose
                                                                            select
                                                                                CheckAndCreateGenericList(args, lastArg,
                                                                                                          expectedLength);


        internal static TypeName ParseString(string typeToParse)
        {
            return TypeDefinition.Parse(typeToParse);
        }

        private static IEnumerable<TypeName> CheckAndCreateGenericList(IEnumerable<TypeName> argList, TypeName lastArg,
                                                                       int expectedLength)
        {
            var result = argList.Concat(new[] {lastArg}).ToArray();
            if (expectedLength != result.Length)
            {
                throw new ArgumentException(
                    string.Format("argument-count hasn't the expected count. List-count {0}, expected {1}",
                                  result.Length, expectedLength));
            }
            return result;
        }

        private static TypeName CreateType(string typeName, string assemblyName, int array,
                                           IEnumerable<TypeName> genericArgs = null)
        {
            if (null == genericArgs)
            {
                genericArgs = new TypeName[0];
            }
            return TypeName.Create(typeName, assemblyName, genericArgs,array);
        }
    }
}