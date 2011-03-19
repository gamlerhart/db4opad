using System;
using System.Collections.Generic;

namespace Gamlor.Db4oPad.MetaInfo
{
    interface IMetaInfo
    {
        IEnumerable<IClassInfo> Classes { get; }
        IDictionary<ITypeDescription, Type> DyanmicTypesRepresentation { get; }
        Type DataContext { get; }
        string ToString();
    }

    interface IClassInfo
    {
        string Name { get; }
        string FullName { get; }
    }
}