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

        public abstract Maybe<Type> KnowsType { get; }

        public abstract IEnumerable<SimpleFieldDescription> Fields { get; }

        public abstract int GenericParametersCount { get; }

        public ITypeDescription BaseClass { get; private set; }

        public bool IsArray
        {
            get { return ArrayOf.HasValue; }
        }

        public abstract Maybe<ITypeDescription> ArrayOf { get; }
    }
}