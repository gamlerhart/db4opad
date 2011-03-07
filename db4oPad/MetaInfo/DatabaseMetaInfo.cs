using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Db4objects.Db4o;

namespace Gamlor.Db4oPad.MetaInfo
{
    class DatabaseMetaInfo
    {
        internal DatabaseMetaInfo(IEnumerable<ITypeDescription> types,
            IDictionary<ITypeDescription, Type> typeMapping)
        {
            Types = types;
            DyanmicTypesRepresentation = typeMapping;
        }

        public static DatabaseMetaInfo Create(IEnumerable<ITypeDescription> types)
        {
            var dynamicRepresentaton = CodeGenerator.Create(types);
            return new DatabaseMetaInfo(types, dynamicRepresentaton);
        }
        public static DatabaseMetaInfo Create(IObjectContainer db)
        {
            var metaInfo = MetaDataReader.Read(db);
            var dynamicRepresentaton = CodeGenerator.Create(metaInfo);
            return new DatabaseMetaInfo(metaInfo, dynamicRepresentaton);
        }

        public IEnumerable<ITypeDescription> Types { get; private set; }
        public IDictionary<ITypeDescription, Type> DyanmicTypesRepresentation { get; private set; }
    }
}