using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Gamlor.Db4oPad.MetaInfo
{
    public class ContextRoot
    {
        public static void Store(object toStore)
        {
            CurrentContext.Store(toStore);
        }
    }
    internal static class ContextTypeGenerator
    {
        private const string MetaDataProperty = "MetaData";

        public const string QueryContextClassName = "LINQPad.User.TypedDataContext";
        private const string MetaDataNameSpace= "LINQPad.User.MetaData";
        private const string MetaDataClassName = MetaDataNameSpace + ".Repo";

        internal static Type CreateContextType(ModuleBuilder builder,
            IEnumerable<KeyValuePair<ITypeDescription, Type>> types)
        {
            var typeBuilder = builder.DefineType(QueryContextClassName,
                                                 PublicClass(), typeof(ContextRoot));
            CreateQueryEntryPoints(types, typeBuilder);
            CreateMetaDataStructure(types, builder, typeBuilder);
            return typeBuilder.CreateType();
        }

        private static void CreateQueryEntryPoints(IEnumerable<KeyValuePair<ITypeDescription, Type>> types,
            TypeBuilder typeBuilder)
        {
            foreach (var type in AllBusinessObjects(types))
            {
                var querableType = typeof(ExtendedQueryable<>).MakeGenericType(type.Value);
                var property = DefineProperty(typeBuilder, type.Key.Name, querableType);
               CreateQueryGetter(type.Value, querableType, property, typeBuilder);
            }
        }

        private static PropertyBuilder DefineProperty(TypeBuilder typeBuilder,
            string name, Type querableType)
        {
            return typeBuilder.DefineProperty(name,
                                              PropertyAttributes.HasDefault,
                                              querableType, null);
        }

        private static void CreateMetaDataStructure(IEnumerable<KeyValuePair<ITypeDescription, Type>> types,
            ModuleBuilder moduleBuilder, TypeBuilder contextTypeBuilder)
        {
            var metaInfoType = CreateMetaInfoProperty(moduleBuilder, types);
            var constructorOfMetaInfoType = metaInfoType.GetConstructors().Single();

            var property = DefineProperty(contextTypeBuilder, MetaDataProperty, metaInfoType);

            var getterMethod = GetterMethodFor(contextTypeBuilder, property, StaticPublicGetter());
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
                property.SetGetMethod(CreateMetaDataGetter(type.Key, typeBuilder, buildInfoType));
            }
            return typeBuilder.CreateType();
        }

        private static Type BuildMetaInfoType(ModuleBuilder moduleBuilder, ITypeDescription type)
        {
            var typeBuilder =moduleBuilder.DefineType(MetaDataNameSpace + "." + type.Name, PublicClass());
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

        private static void CreateQueryGetter(Type type, Type queryableType,
                                                       PropertyBuilder property,
                                                       TypeBuilder typeBuilder)
        {
            var getterMethod = GetterMethodFor(typeBuilder, property, StaticPublicGetter());

            var call = Expression.Call(null, StaticQueryCall(type));
            var lambda = Expression.Lambda(typeof(Func<>).MakeGenericType(queryableType),
                call, new ParameterExpression[0]);
            lambda.CompileToMethod(getterMethod);
        }

        private static MethodBuilder CreateMetaDataGetter(ITypeDescription forType,
            TypeBuilder typeBuilder, Type buildInfoType)
        {
            var typeName = forType.Name;
            var getterMethod = typeBuilder.DefineMethod("get_" + typeName,
                                            PublicGetter(),typeof(string),null);

            var ilGenerator = getterMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Newobj, buildInfoType.GetConstructors().Single());
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
    }
}