using System.Collections.Generic;

namespace Gamlor.Db4oPad.MetaInfo
{
    interface IMetaInfo
    {
        IEnumerable<IClassInfo> Classes { get; }
    }

    interface IClassInfo
    {
        string Name { get; }
        string FullName { get; }
    }
}