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
        int GenericParametersCount { get; }
        Maybe<Type> KnowsType { get; }
    }
}