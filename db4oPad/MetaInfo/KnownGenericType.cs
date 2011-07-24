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
                                  IEnumerable<ITypeDescription> genericArgumentTypes)
            : base(typeInfo, CreateTypeName(typeInfo, genericArgumentTypes), fieldsInitializer)
        {
            this.genericArgumentTypes = genericArgumentTypes;
        }


        public override Maybe<Type> TryResolveType(Func<ITypeDescription, Type> typeResolver)
        {
            if(typeInfo.IsGenericTypeDefinition)
            {
                var genericArguments = genericArgumentTypes.Select(typeResolver).ToArray();
                return typeInfo.MakeGenericType(genericArguments);
            }
            return typeInfo;
        }

        private static TypeName CreateTypeName(Type type, IEnumerable<ITypeDescription> genericArgumentTypes)
        {
            return TypeName.Create(type.FullName.Split('`').First(),
                type.Assembly.GetName().Name,
                genericArgumentTypes.Select(t=>t.TypeName));
        }
    }

    class GenericVariable : KnownType
    {
        internal GenericVariable(Type typeInfo)
            : base(typeInfo, MetaInfo.TypeName.Create(typeInfo.Name,typeInfo.Assembly.GetName().Name),f=>new SimpleFieldDescription[0] )
        {
        }


    }
}