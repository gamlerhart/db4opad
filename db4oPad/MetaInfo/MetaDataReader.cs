using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Db4objects.Db4o.Reflect;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class MetaDataReader
    {
        private readonly Func<TypeName, Maybe<Type>> typeResolver;

        public MetaDataReader(Func<TypeName, Maybe<Type>> typeResolver)
        {
            this.typeResolver = typeResolver;
        }

        public static IEnumerable<ITypeDescription> Read(IObjectContainer database)
        {
            new { database }.CheckNotNull();


            var allKnownClasses = database.Ext().KnownClasses().Distinct().ToArray();
            return CreateTypes(allKnownClasses);
        }

        private static IEnumerable<ITypeDescription> CreateTypes(IEnumerable<IReflectClass> allClasses)
        {
            var typeMap = new Dictionary<string, ITypeDescription>();
            foreach (var classInfo in allClasses)
            {
                CreateType(classInfo, typeMap);
            }
            return typeMap.Select(t => t.Value);
        }

        private static ITypeDescription CreateType(IReflectClass classInfo,
                                                   IDictionary<string, ITypeDescription> knownTypes)
        {
            var name = TypeNameParser.ParseString(classInfo.GetName());
            if (IsSystemType(name))
            {
                var systemType = new SystemType(ResolveType(name));
                knownTypes[name.FullName] = systemType;
                return systemType;
            }
            return name.ArrayOf.Convert(
                n => CreateArrayType(name, classInfo, knownTypes))
                .GetValue(() =>
                    CreateType(name, classInfo, knownTypes));
        }

        private static SimpleClassDescription CreateType(TypeName name,
                                                         IReflectClass classInfo,
                                                         IDictionary<string, ITypeDescription> knownTypes)
        {
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

        private static ITypeDescription CreateArrayType(TypeName fullName,
            IReflectClass classInfo,
                                                        IDictionary<string, ITypeDescription> knownTypes)
        {
            var innerType = GetOrCreateType(classInfo.GetComponentType(),knownTypes);
            var type = ArrayDescription.Create(innerType, fullName.OrderOfArray);
            knownTypes[fullName.FullName] = type;
            return type;
        }

        private static Type ResolveType(TypeName nameAndAssembly)
        {
            var assembly = Assembly.LoadWithPartialName(nameAndAssembly.AssemblyName);
            return assembly.GetType(BuildName(nameAndAssembly));
        }

        private static string BuildName(TypeName name)
        {
            return name.IsGeneric
                       ? string.Format("{0}`{1}", name.Name, name.GenericArguments.Count())
                       : string.Format("{0}", name.Name);
        }

        private static bool IsSystemType(TypeName name)
        {
            return (name.Name.StartsWith("System.") 
                || name.Name.StartsWith("Db4objects.Db4o.")) && name.OrderOfArray==0;
        }

        private static ITypeDescription GetOrCreateType(IReflectClass typeToFind,
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