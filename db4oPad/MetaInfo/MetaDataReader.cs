using System;
using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Reflect.Net;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal delegate Maybe<Type> TypeResolver(TypeName toFind);
    internal class MetaDataReader
    {
        private readonly TypeResolver typeResolver;
        private readonly IExtObjectContainer container;

        internal static TypeResolver DefaultTypeResolver()
        {
            var resolver = new NetReflector();
            return n => resolver.ForName(n.FullName)
                .AsMaybe().Combine(c => c.MaybeCast<NetClass>()).Convert(rc => rc.GetNetType());
        }

        private MetaDataReader(TypeResolver typeResolver, IExtObjectContainer container)
        {
            this.typeResolver = typeResolver;
            this.container = container;
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
            var reader = new MetaDataReader(typeResolver, database.Ext());
            return reader.CreateTypes(allKnownClasses);
        }

        private IEnumerable<ITypeDescription> CreateTypes(IEnumerable<IReflectClass> allClasses)
        {
            var typeMap = new Dictionary<string, ITypeDescription>();
            foreach (var classInfo in allClasses)
            {
                CreateType(classInfo, typeMap);
            }
            return typeMap.Select(t => t.Value).ToList();
        }

        private ITypeDescription CreateType(IReflectClass classInfo,
                                                   IDictionary<string, ITypeDescription> knownTypes)
        {
            var name = NameOf(classInfo);
            return name.ArrayOf.Convert(
                n => CreateArrayType(name, classInfo, knownTypes))
                .GetValue(() =>
                    CreateType(name, classInfo, knownTypes));
        }

        private ITypeDescription CreateType(TypeName name,
                                            IReflectClass classInfo,
                                            IDictionary<string, ITypeDescription> knownTypes)
        {
            var knownType = typeResolver(name)
                .Otherwise(()=>typeResolver(name.GetGenericTypeDefinition()));
            if (knownType.HasValue)
            {
                var systemType = KnownType.Create(knownType.Value,
                    name.GenericArguments.Select(t => GetOrCreateTypeByName(t, knownTypes)));
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

        private ITypeDescription GetOrCreateTypeByName(Maybe<TypeName> maybe,
            IDictionary<string, ITypeDescription> knownTypes)
        {
            var name = maybe.Value;
            return knownTypes.TryGet(name.FullName)
                .GetValue(() => KnownType.Create(typeResolver(name).Value));
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

        private ITypeDescription GetOrCreateType(IReflectClass typeToFind,
                                                        IDictionary<string, ITypeDescription> knownTypes)
        {
            return knownTypes.TryGet(NameOf(typeToFind).FullName)
                .GetValue(() => CreateType(typeToFind, knownTypes));
        }

        private static TypeName NameOf(IReflectClass typeToFind)
        {
            var name = TypeNameParser.ParseString(typeToFind.GetName());
            if (typeToFind.IsArray() && !name.ArrayOf.HasValue)
            {
                return TypeName.CreateArrayOf(name, 1);
            }
            return name;
        }


        private IEnumerable<SimpleFieldDescription> ExtractFields(IReflectClass classInfo,
                                                                         Func<IReflectClass, ITypeDescription>typeLookUp)
        {
            return classInfo.GetDeclaredFields().Select(f => CreateField(classInfo,f, typeLookUp));
        }

        private SimpleFieldDescription CreateField(IReflectClass declaredOn, IReflectField field,
                                                          Func<IReflectClass, ITypeDescription> typeLookUp)
        {
            var indexState = FindOutIndexState(field, declaredOn);
            return SimpleFieldDescription.Create(field.GetName(),
                typeLookUp(field.GetFieldType()), indexState);
        }

        private IndexingState FindOutIndexState(IReflectField field, IReflectClass declaredOn)
        {
            var classInfo = container.Ext().StoredClass(declaredOn);
            if(null!=classInfo)
            {
                var storedField = classInfo.StoredField(field.GetName(), field.GetFieldType());
                if(null!=storedField)
                {
                    return storedField.HasIndex() ? IndexingState.Indexed : IndexingState.NotIndexed;
                }
            }
            return IndexingState.Unknown;
        }
    }
}