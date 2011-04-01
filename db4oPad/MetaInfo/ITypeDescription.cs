using System;
using System.Collections.Generic;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal interface ITypeDescription
    {
        string Name { get; }
        TypeName TypeName { get; }
        IEnumerable<SimpleFieldDescription> Fields { get; }
        Maybe<Type> TryResolveType(Func<ITypeDescription, Type> typeResolver);
        Maybe<ITypeDescription> BaseClass { get;}
        bool IsArray { get; }
        /// <summary>
        /// Do we consider this type as a real entity?
        /// </summary>
        bool IsBusinessEntity { get; }
    }
}