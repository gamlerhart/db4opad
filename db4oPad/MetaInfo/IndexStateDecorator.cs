using System;
using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    class IndexStateDecorator : ITypeDescription
    {
        private readonly ITypeDescription orignalType;

        private IndexStateDecorator(ITypeDescription orignalType,Maybe<ITypeDescription> baseClass,
            Func<IndexStateDecorator,IEnumerable<SimpleFieldDescription>> newFieldStates)
        {
            this.orignalType = orignalType;
            this.BaseClass = baseClass;
            this.Fields = newFieldStates(this).ToList();
        }

        public static ITypeDescription Decorate(ITypeDescription original,
            Func<TypeName, SimpleFieldDescription, IndexingState> stateResolver)
        {
            new { original, stateResolver }.CheckNotNull();
            return Decorate(original, new Dictionary<ITypeDescription, ITypeDescription>(), stateResolver);
        }

        private static ITypeDescription Decorate(ITypeDescription original,
            IDictionary<ITypeDescription, ITypeDescription> allReadyDecorated,
            Func<TypeName,SimpleFieldDescription,IndexingState> stateResolver)
        {
            if(!original.IsBusinessEntity)
            {
                return original;
            }
            var baseClass = original.BaseClass.Convert(bc => Decorate(bc,allReadyDecorated, stateResolver));
            return new IndexStateDecorator(original,
                baseClass,t=>
                {
                    allReadyDecorated[original] = t;
                    return ChangeState(original, allReadyDecorated, stateResolver);
                } );
        }

        private static IEnumerable<SimpleFieldDescription> ChangeState(ITypeDescription original,
            IDictionary<ITypeDescription, ITypeDescription> allReadyDecorated,
            Func<TypeName, SimpleFieldDescription, IndexingState> stateResolver)
        {
            return original.Fields.Select(f => FieldTransformation(f, allReadyDecorated, stateResolver, original));
        }

        private static SimpleFieldDescription FieldTransformation(SimpleFieldDescription f,
            IDictionary<ITypeDescription, ITypeDescription> allReadyDecorated, 
            Func<TypeName, SimpleFieldDescription, IndexingState> stateResolver,
            ITypeDescription original)
        {
            var decoratedFieldType = allReadyDecorated.TryGet(f.Type)
                .GetValue(() =>
                              {
                                  var decorated = Decorate(f.Type,allReadyDecorated, stateResolver);
                                  allReadyDecorated[f.Type] = decorated;
                                  return decorated;
                              });
            return SimpleFieldDescription.Create(f.Name, decoratedFieldType,
                                                 stateResolver(original.TypeName, f));
        }

        public string Name
        {
            get { return orignalType.Name; }
        }

        public TypeName TypeName
        {
            get { return orignalType.TypeName; }
        }

        public IEnumerable<SimpleFieldDescription> Fields { get; private set; }

        public Maybe<Type> TryResolveType(Func<ITypeDescription, Type> typeResolver)
        {
            return orignalType.TryResolveType(typeResolver);
        }

        public Maybe<ITypeDescription> BaseClass { get; private set; }

        public bool IsArray
        {
            get { return orignalType.IsArray; }
        }

        public bool IsBusinessEntity
        {
            get { return orignalType.IsBusinessEntity; }
        }

        private bool Equals(IndexStateDecorator other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.orignalType, orignalType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (IndexStateDecorator)) return false;
            return Equals((IndexStateDecorator) obj);
        }

        public override int GetHashCode()
        {
            return (orignalType != null ? orignalType.GetHashCode() : 0);
        }
    }
}