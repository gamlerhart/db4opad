using System;
using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Reflect.Net;
using Gamlor.Db4oPad.MetaInfo;

namespace Gamlor.Db4oPad
{
    class DatabaseConfigurator
    {
        private readonly DatabaseMetaInfo info;

        public DatabaseConfigurator(DatabaseMetaInfo info)
        {
            this.info = info;
        }

        public static DatabaseConfigurator Create(DatabaseMetaInfo metaInfo)
        {
            return new DatabaseConfigurator(metaInfo);
        }

        public void Configure(IEmbeddedConfiguration configuration)
        {
            ConfigureReflector(configuration, info.DyanmicTypesRepresentation);
            EnsureTypeMapping(configuration.Common, info.DyanmicTypesRepresentation);
        }

        private void ConfigureReflector(IEmbeddedConfiguration configuration,
            IDictionary<ITypeDescription, Type> types)
        {
            var reflector = DynamicGeneratedTypesReflector.CreateInstance(new NetReflector());
            configuration.Common.ReflectWith(reflector);
            foreach (var typeInfo in types.Where(t=>!t.Key.KnowsType.HasValue))
            {
                reflector.AddType(typeInfo.Key.TypeName.FullName, typeInfo.Value);
            }
        }

        private void EnsureTypeMapping(ICommonConfiguration configuration,
            IDictionary<ITypeDescription, Type> types)
        {
            foreach (var typeMapping in types)
            {
                //AddAlias(configuration, typeMapping);
            }
        }

        private void AddAlias(ICommonConfiguration configuration, KeyValuePair<ITypeDescription, Type> typeMapping)
        {
            var storedName = typeMapping.Key.TypeName.FullName;
            var currentName = NameOfType(typeMapping.Value);
            if (storedName != currentName)
            {
                configuration.AddAlias(new TypeAlias(storedName, currentName));
            }
        }

        private string NameOfType(Type typeName)
        {
            return ReflectPlatform.FullyQualifiedName(typeName);
        }
    }

}