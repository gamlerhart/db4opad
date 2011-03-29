using System.Collections.Generic;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class ArrayDescription : TypeDescriptionBase
    {
        private readonly ITypeDescription innerType;

        private ArrayDescription(TypeName typeName, ITypeDescription innerType)
            : base(typeName, KnownType.Array)
        {
            this.innerType = innerType;
        }

        public static ITypeDescription Create(ITypeDescription innerType, int orderOfArray)
        {
            var name = TypeName.CreateArrayOf(innerType.TypeName, orderOfArray);
            return new ArrayDescription(name, innerType);
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