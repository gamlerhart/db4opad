using System;
using System.Collections.Generic;
using System.Reflection;
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

        public static DatabaseMetaInfo Create(IEnumerable<ITypeDescription> types,
            AssemblyName intoAssembly)
        {
            var dynamicRepresentaton = CodeGenerator.Create(types, intoAssembly);
            return new DatabaseMetaInfo(types, dynamicRepresentaton.Types);
        }
        public static DatabaseMetaInfo Create(IEnumerable<ITypeDescription> types,
            Assembly candidateAssembly)
        {
            var dynamicRepresentaton = CodeGenerator.Create(types, candidateAssembly);
            return new DatabaseMetaInfo(types, dynamicRepresentaton.Types);
        }
        public static DatabaseMetaInfo Create(IObjectContainer db,
            Assembly candidateAssembly)
        {
            var metaInfo = MetaDataReader.Read(db);
            return Create(metaInfo, candidateAssembly);
        }
        public static DatabaseMetaInfo Create(IObjectContainer db,
            AssemblyName intoAssembly)
        {
            var metaInfo = MetaDataReader.Read(db);
            var dynamicRepresentaton = CodeGenerator.Create(metaInfo, intoAssembly);
            return new DatabaseMetaInfo(metaInfo, dynamicRepresentaton.Types);
        }

        public IEnumerable<ITypeDescription> Types { get; private set; }
        public IDictionary<ITypeDescription, Type> DyanmicTypesRepresentation { get; private set; }
    }
}