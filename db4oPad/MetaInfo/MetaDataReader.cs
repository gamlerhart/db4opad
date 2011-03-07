using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Db4objects.Db4o.Reflect;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    class MetaDataReader
    {
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
                CreateType(classInfo.GetName(), allClasses, typeMap);
            }
            return typeMap.Select(t => t.Value);
        }
        private static ITypeDescription CreateType(string name,
            IEnumerable<IReflectClass> allClasses,
            IDictionary<string, ITypeDescription> knownTypes)
        {
            return CreateType(TypeNameParser.ParseString(name), allClasses, knownTypes);
        }

        private static ITypeDescription CreateType(TypeName name,
            IEnumerable<IReflectClass> allClasses,
            IDictionary<string, ITypeDescription> knownTypes)
        {
            if (IsSystemType(name))
            {
                var systemType = new SystemType(ResolveType(name));
                knownTypes[name.FullName] = systemType;
                return systemType;
            }
            return SimpleClassDescription.Create(name,
                t =>
                {
                    knownTypes[name.FullName] = t;
                    return ExtractFields(TypeByName(name, allClasses), typeName => GetOrCreateType(typeName, knownTypes, allClasses));
                });
        }

        private static Type ResolveType(TypeName nameAndAssembly)
        {

            var assembly = Assembly.LoadWithPartialName(nameAndAssembly.AssemblyName);
            return assembly.GetType(BuildName(nameAndAssembly));
        }

        private static string BuildName(TypeName name)
        {
            if (name.IsGeneric)
            {
                return string.Format("{0}`{1}", name.Name, name.GenericArguments.Count());
            }
            else
            {
                return string.Format("{0}", name.Name);
            }
        }

        private static bool IsSystemType(TypeName name)
        {
            return name.Name.StartsWith("System.") || name.Name.StartsWith("Db4objects.Db4o.");
        }

        private static ITypeDescription GetOrCreateType(string typeToFind,
            IDictionary<string, ITypeDescription> knownTypes,
            IEnumerable<IReflectClass> allClasses)
        {

            return knownTypes.TryGet(typeToFind)
                .GetValue(() => CreateType(typeToFind, allClasses, knownTypes));
        }

        private static IReflectClass TypeByName(TypeName typeName, IEnumerable<IReflectClass> allClasses)
        {
            return allClasses.Where(t => t.GetName() == typeName.FullName).Single();
        }

        private static IEnumerable<SimpleFieldDescription> ExtractFields(IReflectClass classInfo,
            Func<string, ITypeDescription> typeLookUp)
        {
            return classInfo.GetDeclaredFields().Select(f => CreateField(f, typeLookUp));
        }

        private static SimpleFieldDescription CreateField(IReflectField field,
            Func<string, ITypeDescription> typeLookUp)
        {
            return SimpleFieldDescription.Create(field.GetName(), typeLookUp(field.GetFieldType().GetName()));
        }
    }

}