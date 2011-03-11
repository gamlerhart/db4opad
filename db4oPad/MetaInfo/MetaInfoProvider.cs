using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamlor.Db4oPad.MetaInfo
{
    class MetaInfoProvider : IMetaInfo
    {
        private readonly IEnumerable<IClassInfo> classInfos;
        private DatabaseMetaInfo fullMetaInfo;

        internal const string HelpClassInfoFor = @"Use this call for information about a certain class. " +
                                                 "Example: Db4oPad.Open(\"databaseFile.db4o\").MetaInfo.ClassInfoFor.Person";

        public static MetaInfoProvider Create(DatabaseMetaInfo metaInfo)
        {
            return new MetaInfoProvider(metaInfo);
        }

        public IEnumerable<IClassInfo> Classes
        {
            get { return classInfos; }
        }

        public IDictionary<ITypeDescription, Type> DyanmicTypesRepresentation
        {
            get { return fullMetaInfo.DyanmicTypesRepresentation; }
        }

        public IEnumerable<ITypeDescription> Types
        {
            get { return fullMetaInfo.Types; }
        }

        public override string ToString()
        {
            return "Meta-Info";
        }

        private MetaInfoProvider(DatabaseMetaInfo metaInfo)
        {
            this.fullMetaInfo = metaInfo;
            this.classInfos = ReadMetaInfo(metaInfo);
        }

        private static IEnumerable<IClassInfo> ReadMetaInfo(DatabaseMetaInfo metaInfo)
        {
            return metaInfo.Types.Where(t => !t.KnowsType.HasValue).Select(ClassInfoAdapter.Adapt).ToArray();
        }




        private class ClassInfoAdapter : IClassInfo
        {
            private ITypeDescription typeDescription;

            private ClassInfoAdapter(ITypeDescription typeDescription)
            {
                this.typeDescription = typeDescription;
            }

            public static IClassInfo Adapt(ITypeDescription description)
            {
                return new ClassInfoAdapter(description);
            }

            public override string ToString()
            {
                return typeDescription.Name;
            }

            public string Name
            {
                get { return typeDescription.Name; }
            }

            public string FullName
            {
                get { return typeDescription.TypeName.NameWithGenerics; }
            }
        }
    }

}