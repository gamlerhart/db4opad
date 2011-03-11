using System;
using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class SimpleClassDescription : ITypeDescription
    {
        private SimpleClassDescription(string name, TypeName fullName, int genericParams)
        {
            this.Name = name;
            this.TypeName = fullName;
            this.GenericParametersCount = genericParams;
        }

        public string Name { get; private set; }
        public TypeName TypeName { get; private set; }



        public IEnumerable<SimpleFieldDescription> Fields { get; private set; }


        public int GenericParametersCount { get; private set; }

        public Maybe<Type> KnowsType
        {
            get { return Maybe<Type>.Empty; }
        }
        public static SimpleClassDescription Create(TypeName fullName)
        {
            return Create(fullName, t => new SimpleFieldDescription[0]);
        }

        public static SimpleClassDescription Create(TypeName fullName,
            Func<ITypeDescription, IEnumerable<SimpleFieldDescription>> fieldGenerator)
        {
            var toConstruct = new SimpleClassDescription(ExtractName(fullName), fullName,
                fullName.GenericArguments.Count());
            toConstruct.Fields = fieldGenerator(toConstruct).ToArray();
            return toConstruct;
        }

        private static string ExtractName(TypeName name)
        {
            return name.Name.Split('.').Last() + GenericCount(name.GenericArguments.Count());
        }

        private static string GenericCount(int count)
        {
            if (count != 0)
            {
                return "_" + count;
            }
            return "";
        }
    }

    internal class SimpleFieldDescription
    {
        private SimpleFieldDescription(string fieldName, ITypeDescription type)
        {
            this.Name = fieldName;
            this.Type = type;
        }

        public string Name { get; private set; }

        public ITypeDescription Type
        {
            get;
            private set;
        }

        public static SimpleFieldDescription Create(string fieldName, ITypeDescription type)
        {
            return new SimpleFieldDescription(fieldName, type);
        }
    }
}