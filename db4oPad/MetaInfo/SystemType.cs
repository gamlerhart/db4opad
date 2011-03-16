using System;
using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    class SystemType : ITypeDescription
    {
        private readonly Type typeInfo;

        public readonly static ITypeDescription Object = new SystemType(typeof (object));
        public SystemType(Type typeInfo)
        {
            new { typeInfo }.CheckNotNull();
            this.typeInfo = typeInfo;
        }

        public Type NativeType
        {
            get { return typeInfo; }
        }

        public string Name
        {
            get { return typeInfo.Name; }
        }

        public TypeName TypeName
        {
            get { return CreateTypeName(typeInfo); }
        }

        private static TypeName CreateTypeName(Type type)
        {
            return TypeName.Create(type.FullName, type.Assembly.GetName().Name, GenericNameArguments(type));
        }

        private static IEnumerable<TypeName> GenericNameArguments(Type type)
        {
            return type.GetGenericArguments().Select(CreateTypeName);
        }

        public IEnumerable<SimpleFieldDescription> Fields
        {
            get { return new SimpleFieldDescription[0]; }
        }

        public int GenericParametersCount
        {
            get { return typeInfo.GetGenericArguments().Length; }
        }

        public Maybe<Type> KnowsType
        {
            get { return typeInfo; }
        }

        public ITypeDescription BaseClass
        {
            get { return typeInfo==typeof(object) ?this:new SystemType(typeInfo.BaseType); }
        }

        public bool Equals(SystemType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.typeInfo, typeInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (SystemType)) return false;
            return Equals((SystemType) obj);
        }

        public override int GetHashCode()
        {
            return (typeInfo != null ? typeInfo.GetHashCode() : 0);
        }

        public static bool operator ==(SystemType left, SystemType right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SystemType left, SystemType right)
        {
            return !Equals(left, right);
        }
    }

}