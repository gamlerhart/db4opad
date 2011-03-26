using System;
using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Reflect.Net;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal delegate Maybe<Type> TypeResolver(TypeName toFind);
    internal class MetaDataReader
    {
        private readonly TypeResolver typeResolver;

        internal static TypeResolver DefaultTypeResolver()
        {
            var resolver = new NetReflector();
            return n => resolver.ForName(n.FullName)
                .AsMaybe().Combine(c => c.MaybeCast<NetClass>()).Convert(rc => rc.GetNetType());
        }

        private MetaDataReader(TypeResolver typeResolver)
        {
            this.typeResolver = typeResolver;
        }
        public static IEnumerable<ITypeDescription> Read(IObjectContainer database)
        {
            return Read(database, DefaultTypeResolver());
        }

        public static IEnumerable<ITypeDescription> Read(IObjectContainer database,
            TypeResolver typeResolver)
        {
            new { database, typeResolver }.CheckNotNull();
            
            var allKnownClasses = database.Ext().KnownClasses().Distinct().ToArray();
            var reader = new MetaDataReader(typeResolver);
            return reader.CreateTypes(allKnownClasses);
        }

        private IEnumerable<ITypeDescription> CreateTypes(IEnumerable<IReflectClass> allClasses)
        {
            var typeMap = new Dictionary<string, ITypeDescription>();
            foreach (var classInfo in allClasses)
            {
                CreateType(classInfo, typeMap);
            }
            return typeMap.Select(t => t.Value);
        }

        private ITypeDescription CreateType(IReflectClass classInfo,
                                                   IDictionary<string, ITypeDescription> knownTypes)
        {
            var name = TypeNameParser.ParseString(classInfo.GetName());
            return name.ArrayOf.Convert(
                n => CreateArrayType(name, classInfo, knownTypes))
                .GetValue(() =>
                    CreateType(name, classInfo, knownTypes));
        }

        private ITypeDescription CreateType(TypeName name,
                                                         IReflectClass classInfo,
                                                         IDictionary<string, ITypeDescription> knownTypes)
        {
            var knownType = typeResolver(name);
            if (knownType.HasValue)
            {
                var systemType = KnownType.Create(knownType.Value);
                knownTypes[name.FullName] = systemType;
                return systemType;
            }
            return SimpleClassDescription.Create(name,
                    GetOrCreateType(classInfo.GetSuperclass(), knownTypes),
                    t =>
                        {
                            knownTypes[name.FullName] = t;
                            return ExtractFields(classInfo,
                                                typeName =>
                                                GetOrCreateType(typeName, knownTypes));
                        });
        }

        private ITypeDescription CreateArrayType(TypeName fullName,
            IReflectClass classInfo,
                                                        IDictionary<string, ITypeDescription> knownTypes)
        {
            var innerType = GetOrCreateType(classInfo.GetComponentType(),knownTypes);
            var type = ArrayDescription.Create(innerType, fullName.OrderOfArray);
            knownTypes[fullName.FullName] = type;
            return type;
        }

        private static string BuildName(TypeName name)
        {
            return name.IsGeneric
                       ? string.Format("{0}`{1}", name.NameAndNamespace, name.GenericArguments.Count())
                       : string.Format("{0}", name.NameAndNamespace);
        }

        private static bool IsSystemType(TypeName name)
        {
            return (name.NameAndNamespace.StartsWith("System.") 
                || name.NameAndNamespace.StartsWith("Db4objects.Db4o.")) && name.OrderOfArray==0;
        }

        private ITypeDescription GetOrCreateType(IReflectClass typeToFind,
                                                        IDictionary<string, ITypeDescription> knownTypes)
        {
            return knownTypes.TryGet(typeToFind.GetName())
                .GetValue(() => CreateType(typeToFind, knownTypes));
        }

        private static IEnumerable<SimpleFieldDescription> ExtractFields(IReflectClass classInfo,
                                                                         Func<IReflectClass, ITypeDescription>
                                                                             typeLookUp)
        {
            return classInfo.GetDeclaredFields().Select(f => CreateField(f, typeLookUp));
        }

        private static SimpleFieldDescription CreateField(IReflectField field,
                                                          Func<IReflectClass, ITypeDescription> typeLookUp)
        {
            return SimpleFieldDescription.Create(field.GetName(), typeLookUp(field.GetFieldType()));
        }
    }
}