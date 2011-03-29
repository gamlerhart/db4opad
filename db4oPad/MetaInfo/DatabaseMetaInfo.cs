using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    class DatabaseMetaInfo
    {

        private DatabaseMetaInfo(IEnumerable<ITypeDescription> types,
            IDictionary<ITypeDescription, Type> typeMapping,Type contextType)
        {
            new { types, typeMapping, contextType }.CheckNotNull();
            Types = types;
            EntityTypes = OnlyEntities(types);
            DyanmicTypesRepresentation = typeMapping;
            DataContext = contextType;
        }

        private static IEnumerable<ITypeDescription> OnlyEntities(IEnumerable<ITypeDescription> types)
        {
            return (from t in types
                   where t.IsBusinessEntity
                    select t).ToList();
        }

        public static DatabaseMetaInfo Create(IEnumerable<ITypeDescription> types,
            AssemblyName intoAssembly)
        {
            var dynamicRepresentaton = CodeGenerator.Create(types, intoAssembly);
            return new DatabaseMetaInfo(types, dynamicRepresentaton.Types,dynamicRepresentaton.DataContext);
        }

        public static DatabaseMetaInfo Create(IObjectContainer db,
            TypeResolver typeResolver,
            Assembly candidateAssembly)
        {
            var metaInfo = MetaDataReader.Read(db, typeResolver);
            var dynamicRepresentaton = CodeGenerator.Create(metaInfo, candidateAssembly);
            return new DatabaseMetaInfo(metaInfo, dynamicRepresentaton.Types, dynamicRepresentaton.DataContext);
        }
        public static DatabaseMetaInfo Create(IObjectContainer db,
            AssemblyName intoAssembly)
        {
            return Create(db, MetaDataReader.DefaultTypeResolver(), intoAssembly);
        }

        public static DatabaseMetaInfo Create(IObjectContainer db,
            TypeResolver typeResolver,
            AssemblyName intoAssembly)
        {
            var metaInfo = MetaDataReader.Read(db, typeResolver);
            var dynamicRepresentaton = CodeGenerator.Create(metaInfo, intoAssembly);
            return new DatabaseMetaInfo(metaInfo, dynamicRepresentaton.Types, dynamicRepresentaton.DataContext);
        }

        public IEnumerable<ITypeDescription> Types { get; private set; }
        public IEnumerable<ITypeDescription> EntityTypes { get; private set; }
        public IDictionary<ITypeDescription, Type> DyanmicTypesRepresentation { get; private set; }
        public Type DataContext{ get; private set;}

    }
}