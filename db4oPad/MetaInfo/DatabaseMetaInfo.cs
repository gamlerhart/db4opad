using System;
using System.Collections.Generic;
using System.Reflection;
using Db4objects.Db4o;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    class DatabaseMetaInfo
    {

        internal DatabaseMetaInfo(IEnumerable<ITypeDescription> types,
            IDictionary<ITypeDescription, Type> typeMapping,Type contextType)
        {
            new { types, typeMapping, contextType }.CheckNotNull();
            Types = types;
            DyanmicTypesRepresentation = typeMapping;
            DataContext = contextType;
        }

        public static DatabaseMetaInfo Create(IEnumerable<ITypeDescription> types,
            AssemblyName intoAssembly)
        {
            var dynamicRepresentaton = CodeGenerator.Create(types, intoAssembly);
            return new DatabaseMetaInfo(types, dynamicRepresentaton.Types,dynamicRepresentaton.DataContext);
        }

        public static DatabaseMetaInfo Create(IObjectContainer db,
            Assembly candidateAssembly)
        {
            var metaInfo = MetaDataReader.Read(db);
            var dynamicRepresentaton = CodeGenerator.Create(metaInfo, candidateAssembly);
            return new DatabaseMetaInfo(metaInfo, dynamicRepresentaton.Types, dynamicRepresentaton.DataContext);
        }
        public static DatabaseMetaInfo Create(IObjectContainer db,
            AssemblyName intoAssembly)
        {
            var metaInfo = MetaDataReader.Read(db);
            var dynamicRepresentaton = CodeGenerator.Create(metaInfo, intoAssembly);
            return new DatabaseMetaInfo(metaInfo, dynamicRepresentaton.Types, dynamicRepresentaton.DataContext);
        }

        public IEnumerable<ITypeDescription> Types { get; private set; }
        public IDictionary<ITypeDescription, Type> DyanmicTypesRepresentation { get; private set; }
        public Type DataContext{ get; private set;}

    }
}