using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class SimpleClassDescription : TypeDescriptionBase
    {
        private IEnumerable<SimpleFieldDescription> fields;


        private SimpleClassDescription(string name,
            TypeName fullName, ITypeDescription baseClass) : base(name,fullName,baseClass)
        {
        }


        public override IEnumerable<SimpleFieldDescription> Fields { get { return fields; } }

        
        public override Maybe<ITypeDescription> ArrayOf
        {
            get { return Maybe<ITypeDescription>.Empty; }
        }

        public static SimpleClassDescription Create(TypeName fullName,
            Func<ITypeDescription, IEnumerable<SimpleFieldDescription>> fieldGenerator)
        {
            return Create(fullName,KnownType.Object,fieldGenerator);
        }

        public static SimpleClassDescription Create(TypeName fullName,ITypeDescription baseClass,
            Func<ITypeDescription, IEnumerable<SimpleFieldDescription>> fieldGenerator)
        {
            if (fullName.OrderOfArray != 0)
            {
                throw new ArgumentException("Cannot be an array-type " + fullName.FullName);
            }
            var toConstruct = new SimpleClassDescription(ExtractName(fullName), fullName, baseClass);
            toConstruct.fields = fieldGenerator(toConstruct).ToArray();
            return toConstruct;
        }

        private static string ExtractName(TypeName name)
        {
            var buffer = new StringBuilder(name.Name.Split('.').Last());
            AppendGenericCount(buffer, name.GenericArguments.Count());
            AppendGenericList(buffer,name.GenericArguments);
            return buffer.ToString();

        }

        private static void AppendGenericList(StringBuilder buffer, IEnumerable<TypeName> genericArguments)
        {
            foreach (var arg in genericArguments)
            {
                buffer.Append("_").Append(ExtractName(arg));
            }
        }

        private static void AppendGenericCount(StringBuilder buffer, int count)
        {
            if (count != 0)
            {
                buffer.Append("_").Append(count);
            }
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