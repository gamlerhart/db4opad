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

        protected KnownType(Type typeInfo,
            Func<ITypeDescription,IEnumerable<SimpleFieldDescription>> fieldsInitializer)
        {
            new { typeInfo }.CheckNotNull();
            this.typeInfo = typeInfo;
            this.Fields = fieldsInitializer(this).ToList();
        }
        public static ITypeDescription Create(Type knownType)
        {
            return Create(knownType, new ITypeDescription[0]);
        }
        public static ITypeDescription Create(Type knownType, IEnumerable<ITypeDescription> generics)
        {
            new{knownType,generics}.CheckNotNull();
            return Create(knownType, new Dictionary<Type, ITypeDescription>(), generics);
        }


        public string Name
        {
            get { return typeInfo.Name; }
        }

        public TypeName TypeName
        {
            get { return CreateTypeName(typeInfo); }
        }

        public IEnumerable<SimpleFieldDescription> Fields { get; private set; }

        public virtual Maybe<Type> TryResolveType(Func<ITypeDescription, Type> typeResolver)
        {
            return typeInfo; 
        }

        public ITypeDescription BaseClass
        {
            get { return typeInfo==typeof(object) ?this:KnownType.Create(typeInfo.BaseType); }
        }

        public bool IsArray
        {
            get { return false; }
        }

        public Maybe<ITypeDescription> ArrayOf
        {
            get { return Maybe<ITypeDescription>.Empty; }
        }

        public bool IsBusinessEntity
        {
            get { return TypeDescriptionBase.IsBusinessType(this); }
        }

        static ITypeDescription Create(Type knownType,
            IDictionary<Type, ITypeDescription> knownTypes, IEnumerable<ITypeDescription> generics)
        {
            if (knownType.IsGenericTypeDefinition &&
                knownType.GetGenericArguments().Length != generics.Count())
            {
                throw new ArgumentException("The generic arguments have to match.");
            }
            if(generics.Any())
            {
                return new KnownGenericType(knownType, FieldInitializer(knownType, knownTypes), generics);
            }
            return new KnownType(knownType,
                FieldInitializer(knownType, knownTypes));
        }

        private static Func<ITypeDescription, IEnumerable<SimpleFieldDescription>> FieldInitializer(Type knownType, IDictionary<Type, ITypeDescription> knownTypes)
        {
            return t =>
                       {
                           knownTypes[knownType] = t;
                           return ListFields(knownType, knownTypes);
                       };
        }

        static IEnumerable<SimpleFieldDescription> ListFields(Type type,
            IDictionary<Type, ITypeDescription> knownTypes)
        {
            return type.GetProperties().Select(p => ToFieldDescription(p.PropertyType,p.Name, knownTypes))
                .Union(type.GetFields().Select(f=>ToFieldDescription(f.FieldType,f.Name,knownTypes))).ToList();
        }

        private static SimpleFieldDescription ToFieldDescription(Type fieldType,string name,
            IDictionary<Type, ITypeDescription> knownTypes)
        {
            var type = knownTypes.TryGet(fieldType)
                .GetValue(() => Create(fieldType, knownTypes,new ITypeDescription[0]));
            return SimpleFieldDescription.Create(name, type);
        }

        private TypeName CreateTypeName(Type type)
        {
            return TypeName.Create(type.FullName.Split('`').First(), type.Assembly.GetName().Name, GenericNameArguments(type));
        }

        protected virtual IEnumerable<TypeName> GenericNameArguments(Type type)
        {
            return type.GetGenericArguments().Select(CreateTypeName);
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