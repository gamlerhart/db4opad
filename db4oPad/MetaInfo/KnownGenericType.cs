using System;
using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    class KnownGenericType : KnownType
    {
        private IEnumerable<ITypeDescription> genericArgumentTypes;

        internal KnownGenericType(Type typeInfo,
                                  Func<ITypeDescription, IEnumerable<SimpleFieldDescription>> fieldsInitializer,
                                  IEnumerable<ITypeDescription> genericArgumentTypes) : base(typeInfo, fieldsInitializer)
        {
            this.genericArgumentTypes = genericArgumentTypes;
        }

        public override Maybe<Type> TryResolveType(Func<ITypeDescription, Type> typeResolver)
        {
            var genericArguments = genericArgumentTypes.Select(typeResolver).ToArray();
            return typeInfo.MakeGenericType(genericArguments);
        }

        protected override IEnumerable<TypeName> GenericNameArguments(Type type)
        {
            return genericArgumentTypes.Select(td=>td.TypeName).ToList();
        }
    }
}