using System;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    public abstract class MetaInfoBase
    {
        private ITypeDescription description;

        internal MetaInfoBase(ITypeDescription description)
        {
            new{description}.CheckNotNull();
            this.description = description;
        }

        public override string ToString()
        {
            return description.TypeName.FullName;
        }
    }
}