using System;
using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Config;
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
        }

        private static void ConfigureReflector(ICommonConfigurationProvider configuration,
            IEnumerable<KeyValuePair<ITypeDescription, Type>> types)
        {
            var reflector = DynamicGeneratedTypesReflector.CreateInstance();
            configuration.Common.ReflectWith(reflector);
            foreach (var typeInfo in types.Where(t=>!t.Key.KnowsType.HasValue))
            {
                reflector.AddType(typeInfo.Key.TypeName.FullName, typeInfo.Value);
            }
        }
    }

}