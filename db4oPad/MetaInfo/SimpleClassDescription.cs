using System;
using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class SimpleClassDescription : TypeDescriptionBase
    {
        private IEnumerable<SimpleFieldDescription> fields;


        private SimpleClassDescription(TypeName fullName, Maybe<ITypeDescription> baseClass)
            : base(fullName,baseClass)
        {
        }


        public override IEnumerable<SimpleFieldDescription> Fields { get { return fields; } }

        public override Maybe<Type> TryResolveType(Func<ITypeDescription, Type> typeResolver)
        {
            return Maybe<Type>.Empty;
        }

        public override bool IsArray
        {
            get { return false; }
        }

        public static SimpleClassDescription Create(TypeName fullName,
            Func<ITypeDescription, IEnumerable<SimpleFieldDescription>> fieldGenerator)
        {
            return Create(fullName,Maybe<ITypeDescription>.Empty,fieldGenerator);
        }

        public static SimpleClassDescription Create(TypeName fullName, Maybe<ITypeDescription> baseClass,
            Func<ITypeDescription, IEnumerable<SimpleFieldDescription>> fieldGenerator)
        {
            if (fullName.OrderOfArray != 0)
            {
                throw new ArgumentException("Cannot be an array-type " + fullName.FullName);
            }
            var toConstruct = new SimpleClassDescription(fullName, baseClass);
            toConstruct.fields = fieldGenerator(toConstruct).ToArray();
            return toConstruct;
        }
    }
}