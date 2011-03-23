using System;
using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    class KnownType : ITypeDescription
    {
        private readonly Type typeInfo;

        public readonly static ITypeDescription Object = new KnownType(typeof (object));
        public readonly static ITypeDescription Array = new KnownType(typeof(Array));

        public KnownType(Type typeInfo)
        {
            new { typeInfo }.CheckNotNull();
            this.typeInfo = typeInfo;
        }

        public string Name
        {
            get { return typeInfo.Name; }
        }

        public TypeName TypeName
        {
            get { return CreateTypeName(typeInfo); }
        }

        public IEnumerable<SimpleFieldDescription> Fields
        {
            get { return new SimpleFieldDescription[0]; }
        }

        public Maybe<Type> KnowsType
        {
            get { return typeInfo; }
        }

        public ITypeDescription BaseClass
        {
            get { return typeInfo==typeof(object) ?this:new KnownType(typeInfo.BaseType); }
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