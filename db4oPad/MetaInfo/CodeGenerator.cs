using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class CodeGenerator
    {
        public const string NameSpace = "LINQPad.User";
        public const string QueryContextClassName = "LINQPad.User.TypedDataContext";
        internal static Result Create(IEnumerable<ITypeDescription> metaInfo,
            AssemblyName intoAssembly)
        {
            var assemblyBuilder = CreateAssembly(intoAssembly);
            var builder = CreateModule(assemblyBuilder);
            var dictionary = metaInfo.ToDictionary(mi => mi, mi => Maybe<Type>.Empty);
            var types = CreateTypes(builder, dictionary);
            var contextType = CreateContextType(builder, types);
            assemblyBuilder.Save(Path.GetFileName(intoAssembly.CodeBase));
            return new Result(contextType, types);
        }

        public static Result Create(IEnumerable<ITypeDescription> metaInfo, Assembly candidateAssembly)
        {
            var types = metaInfo.ToDictionary(mi => mi, mi => FindType(mi,candidateAssembly));
            var contextType = candidateAssembly.GetType(QueryContextClassName);
            return new Result(contextType, types);
        }

        private static Type FindType(ITypeDescription typeInfo, Assembly candidateAssembly)
        {
            return typeInfo.KnowsType
                .Convert(t => t)
                .GetValue(() => candidateAssembly.GetType(BuildName(typeInfo.TypeName.NameWithGenerics)));
        }


        private static Type CreateContextType(ModuleBuilder builder, IDictionary<ITypeDescription, Type> types)
        {
            var typeBuilder = builder.DefineType(QueryContextClassName,
                                         TypeAttributes.Class | TypeAttributes.Public);
            foreach (var type in types.Where(t=>!t.Key.KnowsType.HasValue))
            {
                var querableType = typeof (IQueryable<>).MakeGenericType(type.Value);
                var property = typeBuilder.DefineProperty(type.Key.Name,
                                                          PropertyAttributes.HasDefault, 
                                                          querableType, null);
                
                property.SetGetMethod(CreateQueryGetter(type.Value,querableType, type.Key.Name, typeBuilder));
               
            }

            return typeBuilder.CreateType();

        }

        private static MethodBuilder CreateQueryGetter(Type type, Type queryableType,
            string propertyName,
            TypeBuilder typeBuilder)
        {
            var getterMethod =
                typeBuilder.DefineMethod("get_" + propertyName,
                                         MethodAttributes.Public | MethodAttributes.SpecialName |
                                         MethodAttributes.HideBySig|MethodAttributes.Static);

            var call = Expression.Call(null, StaticQueryCall(type));
            var lambda = Expression.Lambda(typeof(Func<>).MakeGenericType(queryableType),
                call, new ParameterExpression[0]);
            lambda.CompileToMethod(getterMethod);
            return getterMethod;
        }

        private static MethodInfo StaticQueryCall(Type forType)
        {
            return typeof (CurrentContext).GetMethod("Query").MakeGenericMethod(forType);
        }

        private static IDictionary<ITypeDescription, Type> CreateTypes(ModuleBuilder modBuilder,
            IDictionary<ITypeDescription, Maybe<Type>> typeBuildMap)
        {
            foreach (var typeInfo in typeBuildMap.Keys.ToArray())
            {
                GetOrCreateType(typeBuildMap, modBuilder, typeInfo);
            }
            return typeBuildMap.ToDictionary(c => c.Key, c => BuildType(c.Value.Value));
        }

        private static Type BuildType(Type value)
        {
            return value.MaybeCast<TypeBuilder>().Convert(tb => tb.CreateType()).GetValue(value);
        }

        private static Type GetOrCreateType(IDictionary<ITypeDescription, Maybe<Type>> typeBuildMap,
                                            ModuleBuilder modBuilder, ITypeDescription typeInfo)
        {
            return typeBuildMap[typeInfo]
                .GetValue(() => CreateType(typeBuildMap, modBuilder, typeInfo));
        }

        private static Type CreateType(IDictionary<ITypeDescription, Maybe<Type>> typeBuildMap,
                                       ModuleBuilder modBuilder, ITypeDescription typeInfo)
        {
            return typeInfo.KnowsType
                .Convert(t => AddNativeType(typeInfo, t, typeBuildMap))
                .GetValue(() => AddNoNativeType(typeInfo, typeBuildMap, modBuilder));
        }

        private static Type AddNoNativeType(ITypeDescription typeInfo,
                                            IDictionary<ITypeDescription, Maybe<Type>> typeBuildMap,
                                            ModuleBuilder modBuilder)
        {
            var defineType = CreateType(modBuilder, typeInfo.TypeName.NameWithGenerics);
            typeBuildMap[typeInfo] = defineType;

            foreach (var field in typeInfo.Fields)
            {
                CreateFields(defineType, field, modBuilder, typeBuildMap);
            }
            return defineType;
        }

        private static Type AddNativeType(ITypeDescription typeInfo, Type type,
                                          IDictionary<ITypeDescription, Maybe<Type>> typeBuildMap)
        {
            typeBuildMap[typeInfo] = type;
            return type;
        }

        private static void CreateFields(TypeBuilder typeBuilder, SimpleFieldDescription field, ModuleBuilder modBuilder,
                                         IDictionary<ITypeDescription, Maybe<Type>> typeBuildMap)
        {
            var type = GetType(field, typeBuildMap, modBuilder);
            var generatedField = typeBuilder.DefineField(field.Name,
                                                         type,
                                                         FieldAttributes.Public);
            AddOptionalProperty(typeBuilder, field, type, generatedField);
        }

        private static void AddOptionalProperty(TypeBuilder typeBuilder,
            SimpleFieldDescription field,
            Type type, FieldBuilder generatedField)
        {
            if (char.IsLower(field.Name[0]))
            {
                CreateProperty(typeBuilder, field, type, generatedField);
            }
        }

        private static void CreateProperty(TypeBuilder typeBuilder, SimpleFieldDescription field,
            Type type, FieldBuilder generatedField)
        {
            var propertyName = char.ToUpperInvariant(field.Name[0]) + field.Name.Substring(1);
            var property = typeBuilder.DefineProperty(propertyName,
                                                      PropertyAttributes.HasDefault,
                                                      type, new[] { type });

            property.SetGetMethod(CreateGetter(typeBuilder, propertyName, generatedField));
            property.SetSetMethod(CreateSetter(typeBuilder, propertyName, generatedField));
        }

        private static MethodBuilder CreateGetter(TypeBuilder typeBuilder, string propertyName,
                                                  FieldBuilder generatedField)
        {
            var getterMethod =
                typeBuilder.DefineMethod("get_" + propertyName,
                                         MethodAttributes.Public | MethodAttributes.SpecialName |
                                         MethodAttributes.HideBySig,
                                         generatedField.FieldType,
                                         Type.EmptyTypes);

            var ilGenerator = getterMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, generatedField);
            ilGenerator.Emit(OpCodes.Ret);
            return getterMethod;
        }

        private static MethodBuilder CreateSetter(TypeBuilder typeBuilder, string propertyName,
                                                  FieldBuilder generatedField)
        {
            var setterMethod
                = typeBuilder.DefineMethod("set_" + propertyName,
                                           MethodAttributes.Public | MethodAttributes.SpecialName |
                                           MethodAttributes.HideBySig,
                                           null,
                                           new[] { generatedField.FieldType });

            var ilGenerator = setterMethod.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, generatedField);
            ilGenerator.Emit(OpCodes.Ret);
            return setterMethod;
        }

        private static Type GetType(SimpleFieldDescription field,
                                    IDictionary<ITypeDescription, Maybe<Type>> typeBuildMap,
            ModuleBuilder modBuilder)
        {
            return typeBuildMap[field.Type].GetValue(
                () => GetOrCreateType(typeBuildMap, modBuilder, field.Type));
        }

        private static TypeBuilder CreateType(ModuleBuilder modBuilder, string className)
        {
            return modBuilder.DefineType(BuildName(className),
                                         TypeAttributes.Class | TypeAttributes.Public);
        }

        private static string BuildName(string className)
        {
            return NameSpace + "." + className;
        }

        private static ModuleBuilder CreateModule(AssemblyBuilder builder)
        {
            var theName = builder.GetName();
            return builder.DefineDynamicModule(theName.Name, theName.Name + ".dll");
        }

        private static AssemblyBuilder CreateAssembly(AssemblyName theName)
        {
            var assembly = AppDomain.CurrentDomain
                .DefineDynamicAssembly(theName, AssemblyBuilderAccess.RunAndSave,
                                       Path.GetDirectoryName(theName.CodeBase));
            return assembly;
        }

        public class Result : IEnumerable<KeyValuePair<ITypeDescription, Type>>
        {
            private readonly IDictionary<ITypeDescription, Type> types;
            private readonly Type dataContext;


            public Result(Type dataContext,IDictionary<ITypeDescription, Type> types)
            {
                this.dataContext = dataContext;
                this.types = types;
            }

            public IEnumerator<KeyValuePair<ITypeDescription, Type>> GetEnumerator()
            {
                return types.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public Type this[ITypeDescription key]
            {
                get { return types[key]; }
            }

            public IDictionary<ITypeDescription, Type> Types
            {
                get { return types; }
            }

            public Type DataContext
            {
                get { return dataContext; }
            }
        }
    }

}