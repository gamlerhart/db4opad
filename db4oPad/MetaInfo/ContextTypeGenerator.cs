using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    public class ContextRoot
    {
        public static void Store(object toStore)
        {
            CurrentContext.Store(toStore);
        }
    }
    internal class ContextTypeGenerator
    {
        private const string MetaDataProperty = "MetaData";

        public const string QueryContextClassName = "LINQPad.User.TypedDataContext";
        private const string MetaDataNameSpace= "LINQPad.User.MetaData";
        private const string MetaDataClassName = MetaDataNameSpace + ".Repo";
        private TypeBuilder rootType;
        private ModuleBuilder moduleBuilder;
        private IDictionary<string,TypeBuilder> nameSpaceHolder = new Dictionary<string, TypeBuilder>();

        public ContextTypeGenerator(ModuleBuilder moduleBuilder, TypeBuilder rootType)
        {
            this.rootType = rootType;
            this.moduleBuilder = moduleBuilder;
        }

        internal static Type CreateContextType(ModuleBuilder builder,
            IEnumerable<KeyValuePair<ITypeDescription, Type>> types)
        {
            var typesGroupesByName = from t in types
                                     group t by t.Key.Name
                                     into byName
                                         select new ByNameGrouping(byName.Key,byName);
            var typeBuilder = builder.DefineType(QueryContextClassName,
                                                 PublicClass(), typeof(ContextRoot));
            return new ContextTypeGenerator(builder,typeBuilder).Build(typesGroupesByName, types);
        }

        private Type Build(IEnumerable<ByNameGrouping> typesGroupesByName,
             IEnumerable<KeyValuePair<ITypeDescription, Type>> types)
        {
            CreateQueryEntryPoints(typesGroupesByName);
            CreateMetaDataStructure(types);
            return rootType.CreateType();
        }

        private void CreateQueryEntryPoints(IEnumerable<ByNameGrouping> types)
        {
            foreach (var type in types)
            {
                CreateQueryEntryPoints(type);
            }
            FinishNamespaces();
        }

        private void FinishNamespaces()
        {
            foreach (var typeBuilder in nameSpaceHolder.Values)
            {
                typeBuilder.CreateType();
            }
        }

        private void CreateQueryEntryPoints(ByNameGrouping type)
        {
            foreach (var typeToBuildFor in AllBusinessObjects(type.Members))
            {
                var definePropertyOn = FindLocationForProperty(type,typeToBuildFor.Key);
                var querableType = typeof(ExtendedQueryable<>).MakeGenericType(typeToBuildFor.Value);
                var property = DefineProperty(definePropertyOn, typeToBuildFor.Key.Name, querableType);
                CreateQueryGetter(typeToBuildFor.Value, property, definePropertyOn);
            }
        }

        private TypeBuilder FindLocationForProperty(ByNameGrouping type,
            ITypeDescription forType)
        {
            if(type.HasMultipleValues)
            {
                var typeNameSeparation = forType.TypeName.NameAndNamespace.LastIndexOf('.');
                var namespaceName = forType.TypeName.NameAndNamespace.Substring(0, typeNameSeparation);
                return GetOrCreateNamespaceFor(namespaceName);
            }
            else
            {
                return rootType;
            }
            
        }

        private TypeBuilder GetOrCreateNamespaceFor(string namespaceName)
        {
            var currentNamespace = "";
            foreach (var ns in namespaceName.Split('.'))
            {
                var lastNameSpace = currentNamespace;
                currentNamespace = currentNamespace + (currentNamespace.Length!=0 ? "." : "") + ns;
                var clsFixNS = currentNamespace; 
                this.nameSpaceHolder.TryGet(currentNamespace)
                    .GetValue(() => BuildNamespaceContext(lastNameSpace, clsFixNS,ns));
            }
            return nameSpaceHolder[currentNamespace];
        }

        private TypeBuilder BuildNamespaceContext(string lastNameSpace,
            string forNamespace,string ns)
        {
            var currentType = moduleBuilder.DefineType(CodeGenerator.NameSpace+ "."+ forNamespace + ".NameSpaceContext", PublicClass());
            var parentNS = nameSpaceHolder.TryGet(lastNameSpace).GetValue(rootType);

            var accessType = QueryPropertyAccessRights(parentNS);

            var property = DefineProperty(parentNS, ns, currentType);
            var constructor = currentType.DefineConstructor(PublicConstructurSignature(), CallingConventions.Standard, new Type[0]);
            var ilGenerator = constructor.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call,typeof(object).GetConstructors().Single());
            ilGenerator.Emit(OpCodes.Ret);
            property.SetGetMethod(CreateNewInstanceGetter(ns, parentNS, constructor, accessType));

            nameSpaceHolder[forNamespace] = currentType;
            return currentType;
        }

        private static PropertyBuilder DefineProperty(TypeBuilder typeBuilder,
            string name, Type type)
        {
            return typeBuilder.DefineProperty(name,
                                              PropertyAttributes.HasDefault,
                                              type, null);
        }

        private void CreateMetaDataStructure(IEnumerable<KeyValuePair<ITypeDescription, Type>> types)
        {
            var metaInfoType = CreateMetaInfoProperty(moduleBuilder, types);
            var constructorOfMetaInfoType = metaInfoType.GetConstructors().Single();

            var property = DefineProperty(rootType, MetaDataProperty, metaInfoType);

            var getterMethod = GetterMethodFor(rootType, property, StaticPublicGetter());
            var lambda = Expression.Lambda(typeof(Func<>).MakeGenericType(metaInfoType),
                                           Expression.New(constructorOfMetaInfoType), new ParameterExpression[0]);
            lambda.CompileToMethod(getterMethod);
        }

        private static Type CreateMetaInfoProperty(ModuleBuilder moduleBuilder, 
            IEnumerable<KeyValuePair<ITypeDescription, Type>> types)
        {
            var typeBuilder = moduleBuilder.DefineType(MetaDataClassName, PublicClass());
            foreach (var type in AllBusinessObjects(types))
            {
                var property = DefineProperty(typeBuilder,type.Key.Name,typeof(string));
                var buildInfoType = BuildMetaInfoType(moduleBuilder, type.Key);
                property.SetGetMethod(CreateNewInstanceGetter(type.Key.Name,
                    typeBuilder,
                    buildInfoType.GetConstructors().Single(),
                    PublicGetter()));
            }
            return typeBuilder.CreateType();
        }

        private static Type BuildMetaInfoType(ModuleBuilder moduleBuilder, ITypeDescription type)
        {
            var typeBuilder = moduleBuilder.DefineType(MetaDataNameSpace + "." + type.TypeName.NameWithGenerics, PublicClass());
            AddNameProperty(typeBuilder, type);
            return typeBuilder.CreateType();
        }

        private static void AddNameProperty(TypeBuilder typeBuilder, ITypeDescription type)
        {
            AddLabelProperty(typeBuilder, "ClassName", type.Name);
            AddLabelProperty(typeBuilder, "ClassFullName", type.TypeName.FullName);
            foreach (var field in type.Fields)
            {
                AddLabelProperty(typeBuilder, field.AsPropertyName(), NameWithIndexState(field.Name, field.IndexingState));
                if(!field.IsBackingField)
                {
                    AddLabelProperty(typeBuilder, field.Name, NameWithIndexState(field.Name,field.IndexingState));
                }
            }
        }

        private static string NameWithIndexState(string name, IndexingState indexingState)
        {
            return string.Format("{0} (Index: {1})", name, indexingState);
        }

        private static void AddLabelProperty(TypeBuilder typeBuilder,string propertyName,
            string valueToReturn)
        {
            var property = DefineProperty(typeBuilder, propertyName, typeof(string));
            var methodBuilder = GetterMethodFor(typeBuilder, property,PublicGetter());
            
            var ilCode = methodBuilder.GetILGenerator();
            ilCode.Emit(OpCodes.Ldstr, valueToReturn);
            ilCode.Emit(OpCodes.Ret);
        }

        private void CreateQueryGetter(Type type,
                                                       PropertyBuilder property,
                                                       TypeBuilder typeBuilder)
        {
            var getterMethod = GetterMethodFor(typeBuilder, property, QueryPropertyAccessRights(typeBuilder));
            var ilGenerator = getterMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Call, StaticQueryCall(type));
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static MethodBuilder CreateNewInstanceGetter(string getterName,
            TypeBuilder typeBuilder, ConstructorInfo constructor, MethodAttributes accessRights)
        {
            var getterMethod = typeBuilder.DefineMethod("get_" + getterName,
                                            accessRights, constructor.DeclaringType, null);

            var ilGenerator = getterMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Newobj, constructor);
            ilGenerator.Emit(OpCodes.Ret);
            return getterMethod;
        }

        private static MethodBuilder GetterMethodFor(TypeBuilder typeBuilder,
            PropertyBuilder property,MethodAttributes access)
        {
            var getterMethod = typeBuilder.DefineMethod("get_" + property.Name,
                                            access, property.PropertyType, null);

            property.SetGetMethod(getterMethod);
            return getterMethod;
        }

        private MethodAttributes QueryPropertyAccessRights(TypeBuilder parentNS)
        {
            return parentNS == rootType ? StaticPublicGetter() : PublicGetter();
        }

        private static MethodAttributes PublicConstructurSignature()
        {
            return MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        }

        private static MethodAttributes StaticPublicGetter()
        {
            return MethodAttributes.Public | MethodAttributes.SpecialName |
                   MethodAttributes.HideBySig | MethodAttributes.Static;
        }

        private static MethodAttributes PublicGetter()
        {
            return MethodAttributes.Public | MethodAttributes.SpecialName |
                   MethodAttributes.HideBySig;
        }


        private static TypeAttributes PublicClass()
        {
            return TypeAttributes.Class | TypeAttributes.Public;
        }

        private static IEnumerable<KeyValuePair<ITypeDescription, Type>> AllBusinessObjects(IEnumerable<KeyValuePair<ITypeDescription, Type>> types)
        {
            return types.Where(t=>t.Key.IsBusinessEntity);
        }


        private static MethodInfo StaticQueryCall(Type forType)
        {
            return typeof(CurrentContext).GetMethod("Query").MakeGenericMethod(forType);
        }

        class ByNameGrouping
        {
            public ByNameGrouping(string simpleName, 
                IEnumerable<KeyValuePair<ITypeDescription, Type>> members)
            {
                SimpleName = simpleName;
                Members = members.ToList();
            }

            public string SimpleName { get; private set; }
            public IEnumerable<KeyValuePair<ITypeDescription, Type>> Members { get; private set; }
            public bool HasMultipleValues { get {return Members.Count() > 1; } }
        }
    }
}