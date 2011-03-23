using System;
using System.Collections.Generic;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class ArrayDescription : TypeDescriptionBase
    {
        private readonly ITypeDescription innerType;

        public ArrayDescription(string name,
            TypeName typeName, ITypeDescription innerType)
            : base(name, typeName, KnownTypes.Array)
        {
            this.innerType = innerType;
        }

        public static ITypeDescription Create(ITypeDescription innerType, int orderOfArray)
        {
            var name = TypeName.CreateArrayOf(innerType.TypeName, orderOfArray);
            return new ArrayDescription(name.Name, name, innerType);
        }

        public override IEnumerable<SimpleFieldDescription> Fields
        {
            get { return new SimpleFieldDescription[0]; }
        }

        public override Maybe<ITypeDescription> ArrayOf
        {
            get { return Maybe.From(innerType); }
        }
    }
}