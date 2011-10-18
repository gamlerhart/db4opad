using System;
using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    class KnownType : ITypeDescription
    {
        protected readonly Type typeInfo;

        private static readonly List<SimpleFieldDescription> EmptyFieldList = new List<SimpleFieldDescription>();
        public readonly static ITypeDescription Object = new KnownType(typeof (object),f=>EmptyFieldList);
        public readonly static ITypeDescription Array = new KnownType(typeof(Array), f => EmptyFieldList);
        public readonly static ITypeDescription String = new KnownType(typeof(string), f => EmptyFieldList);

        protected KnownType(Type typeInfo, TypeName theName,
            Func<ITypeDescription,IEnumerable<SimpleFieldDescription>> fieldsInitializer)
        {
            new { typeInfo }.CheckNotNull();
            this.Name = theName.NameWithGenerics.Split('.').Last();
            this.typeInfo = typeInfo;
            this.TypeName = theName; 
            this.Fields = fieldsInitializer(this).ToList();
        }
        private KnownType(Type typeInfo, 
            Func<ITypeDescription,IEnumerable<SimpleFieldDescription>> fieldsInitializer) : this(typeInfo,CreateTypeName(typeInfo) ,fieldsInitializer)
        {
        }
        public static ITypeDescription Create(Type knownType)
        {
            return Create(knownType, new ITypeDescription[0],(d,fn,ft)=>IndexingState.Unknown);
        }
        public static ITypeDescription Create(Type knownType,
            IEnumerable<ITypeDescription> generics)
        {
            return Create(knownType, generics, (d, fn, ft) => IndexingState.Unknown);
        }
        public static ITypeDescription Create(Type knownType,
            IEnumerable<ITypeDescription> generics,IndexStateLookup indexLookUp)
        {
            new{knownType,generics}.CheckNotNull();
            return Create(knownType, 
                new Dictionary<Type, ITypeDescription>(),
                generics, indexLookUp);
        }


        public string Name { get; private set; }

        public TypeName TypeName { get; private set; }

        public IEnumerable<SimpleFieldDescription> Fields { get; private set; }

        public virtual Maybe<Type> TryResolveType(Func<ITypeDescription, Type> typeResolver)
        {
            return typeInfo; 
        }

        public Maybe<ITypeDescription> BaseClass
        {
            get { return typeInfo.BaseType.AsMaybe().Convert(Create); }
        }

        public bool IsArray
        {
            get { return false; }
        }

        public bool IsBusinessEntity
        {
            get { return TypeDescriptionBase.IsBusinessType(this); }
        }

        static ITypeDescription Create(Type knownType,
            IDictionary<Type, ITypeDescription> knownTypes,
            IEnumerable<ITypeDescription> generics,
            IndexStateLookup indexLookUp)
        {
            if (knownType.IsGenericTypeDefinition &&
                knownType.GetGenericArguments().Length != generics.Count())
            {
                throw new ArgumentException("The generic arguments have to match.");
            }
            if(generics.Any())
            {
                return new KnownGenericType(knownType, FieldInitializer(knownType, knownTypes, indexLookUp), generics);
            }
            if (knownType.IsGenericParameter)
            {
                return new GenericVariable(knownType);
            }
            return new KnownType(knownType, CreateTypeName(knownType),
                FieldInitializer(knownType, knownTypes, indexLookUp));
        }

        private static Func<ITypeDescription, IEnumerable<SimpleFieldDescription>> FieldInitializer(Type knownType,
            IDictionary<Type, ITypeDescription> knownTypes, IndexStateLookup indexLookUp)
        {
            return t =>
                       {
                           knownTypes[knownType] = t;
                           return ListFields(t.TypeName,knownType, knownTypes, indexLookUp);
                       };
        }

        static IEnumerable<SimpleFieldDescription> ListFields(TypeName declaringType,Type type,
            IDictionary<Type, ITypeDescription> knownTypes, IndexStateLookup indexLookUp)
        {
            return type.GetProperties().Select(p => ToFieldDescription(declaringType,p.PropertyType, p.Name, knownTypes, indexLookUp))
                .Union(type.GetFields().Select(f => ToFieldDescription(declaringType,f.FieldType, f.Name, knownTypes, indexLookUp))).ToList();
        }

        private static SimpleFieldDescription ToFieldDescription(TypeName declaringType,Type fieldType,string name,
            IDictionary<Type, ITypeDescription> knownTypes, IndexStateLookup indexLookUp)
        {
            var type = knownTypes.TryGet(fieldType)
                .GetValue(() => Create(fieldType, knownTypes,new ITypeDescription[0],indexLookUp));
            return SimpleFieldDescription.Create(name, type, indexLookUp(declaringType, name, type.TypeName));
        }

        private static TypeName CreateTypeName(Type type)
        {
            var name = type.Namespace +"." + 
                (type.DeclaringType!=null?type.DeclaringType.Name+"+":"")
                + type.Name;
            return TypeName.Create(name, 
                type.Assembly.GetName().Name,
                type.GetGenericArguments().Select(CreateTypeName));
        }

        private bool Equals(KnownType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.typeInfo, typeInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (KnownType)) return false;
            return Equals((KnownType) obj);
        }

        public override int GetHashCode()
        {
            return (typeInfo != null ? typeInfo.GetHashCode() : 0);
        }

        public static bool operator ==(KnownType left, KnownType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(KnownType left, KnownType right)
        {
            return !Equals(left, right);
        }
    }
}