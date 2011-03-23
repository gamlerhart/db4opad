using System;
using System.Collections.Generic;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal abstract class TypeDescriptionBase : ITypeDescription
    {
        protected TypeDescriptionBase(string name, TypeName typeName, ITypeDescription baseClass)
        {
            Name = name;
            TypeName = typeName;
            BaseClass = baseClass;
        }

        public string Name { get; private set; }
        public TypeName TypeName { get; private set; }

        public virtual Maybe<Type> KnowsType
        {
            get { return Maybe<Type>.Empty; }
        }

        public abstract IEnumerable<SimpleFieldDescription> Fields { get; }

        public ITypeDescription BaseClass { get; private set; }

        public bool IsArray
        {
            get { return ArrayOf.HasValue; }
        }

        public abstract Maybe<ITypeDescription> ArrayOf { get; }

        public bool IsBusinessEntity
        {
            get {
                return IsBusinessType(this);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(TypeDescriptionBase other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.TypeName, TypeName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is TypeDescriptionBase)) return false;
            return Equals((TypeDescriptionBase) obj);
        }

        public override int GetHashCode()
        {
            return (TypeName != null ? TypeName.GetHashCode() : 0);
        }

        internal static bool IsBusinessType(ITypeDescription description)
        {
            var name = description.TypeName.FullName;
            return !description.IsArray &&
                   !name.StartsWith("System.")
                   && !name.StartsWith("Db4objects.");
        }
    }
}