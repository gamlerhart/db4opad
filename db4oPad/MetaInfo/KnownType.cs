using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    class KnownType : ITypeDescription
    {
        private readonly Type typeInfo;

        private static readonly List<SimpleFieldDescription> EmptyFieldList = new List<SimpleFieldDescription>();
        public readonly static ITypeDescription Object = new KnownType(typeof (object),f=>EmptyFieldList);
        public readonly static ITypeDescription Array = new KnownType(typeof(Array), f => EmptyFieldList);
        public readonly static ITypeDescription String = new KnownType(typeof(string), f => EmptyFieldList);

        private KnownType(Type typeInfo,
            Func<ITypeDescription,IEnumerable<SimpleFieldDescription>> fieldsInitializer)
        {
            new { typeInfo }.CheckNotNull();
            this.typeInfo = typeInfo;
            this.Fields = fieldsInitializer(this).ToList();;
        }
        public static ITypeDescription Create(Type knownType)
        {
            return Create(knownType,new Dictionary<Type, ITypeDescription>());
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

        public Maybe<Type> KnowsType
        {
            get { return typeInfo; }
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
            IDictionary<Type, ITypeDescription> knownTypes)
        {
            return new KnownType(knownType,
                t =>
                {
                    knownTypes[knownType] = t;
                    return ListFields(knownType, knownTypes);
                });
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
                .GetValue(() => Create(fieldType, knownTypes));
            return SimpleFieldDescription.Create(name, type);
        }

        private static TypeName CreateTypeName(Type type)
        {
            return TypeName.Create(type.FullName, type.Assembly.GetName().Name, GenericNameArguments(type));
        }

        private static IEnumerable<TypeName> GenericNameArguments(Type type)
        {
            return type.GetGenericArguments().Select(CreateTypeName);
        }

        public bool Equals(KnownType other)
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