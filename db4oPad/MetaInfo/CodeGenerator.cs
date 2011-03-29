using System;
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

        internal static CodeGenerationResult Create(IEnumerable<ITypeDescription> metaInfo,
            AssemblyName intoAssembly)
        {
            var assemblyBuilder = CreateAssembly(intoAssembly);
            var builder = CreateModule(assemblyBuilder);
            var types = CreateTypes(builder, metaInfo);
            var contextType = CreateContextType(builder, types);
            assemblyBuilder.Save(Path.GetFileName(intoAssembly.CodeBase));
            return new CodeGenerationResult(contextType, types);
        }

        public static CodeGenerationResult Create(IEnumerable<ITypeDescription> metaInfo, Assembly candidateAssembly)
        {
            var types = metaInfo.ToDictionary(mi => mi, mi => FindType(mi,candidateAssembly));
            var contextType = candidateAssembly.GetType(QueryContextClassName);
            return new CodeGenerationResult(contextType, types);
        }

        private static Type FindType(ITypeDescription typeInfo, Assembly candidateAssembly)
        {
            return typeInfo.KnowsType
                .Convert(t => t)
                .GetValue(
                () => FindTypeOrArray(typeInfo, candidateAssembly));
        }

        private static Type FindTypeOrArray(ITypeDescription typeInfo, Assembly candidateAssembly)
        {
            return typeInfo.ArrayOf.Convert(t =>FindType(t, candidateAssembly).MakeArrayType())
                .GetValue(() => candidateAssembly.GetType(BuildName(typeInfo.TypeName.NameWithGenerics)));
        }


        private static Type CreateContextType(ModuleBuilder builder, IDictionary<ITypeDescription, Type> types)
        {
            var typeBuilder = builder.DefineType(QueryContextClassName,
                                         TypeAttributes.Class | TypeAttributes.Public);
            foreach (var type in types.Where(t=>t.Key.IsBusinessEntity))
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
            IEnumerable<ITypeDescription> typesToBuild)
        {
            var typeBuildMap = new Dictionary<ITypeDescription, Type>();
            foreach (var typeInfo in typesToBuild)
            {
                GetOrCreateType(typeBuildMap, modBuilder, typeInfo);
            }
            return typeBuildMap.ToDictionary(c => c.Key, c => BuildType(c.Value));
        }

        private static Type BuildType(Type value)
        {
            return value.MaybeCast<TypeBuilder>().Convert(tb => tb.CreateType()).GetValue(value);
        }

        private static Type GetOrCreateType(IDictionary<ITypeDescription, Type> typeBuildMap,
                                            ModuleBuilder modBuilder, ITypeDescription typeInfo)
        {
            return typeBuildMap.TryGet(typeInfo)
                .GetValue(() => CreateType(typeBuildMap, modBuilder, typeInfo));
        }

        private static Type CreateType(IDictionary<ITypeDescription, Type> typeBuildMap,
                                       ModuleBuilder modBuilder, ITypeDescription typeInfo)
        {
            return typeInfo.KnowsType
                .Convert(t => AddNativeType(typeInfo, t, typeBuildMap))
                .GetValue(() => ArrayOrNewType(typeInfo, typeBuildMap, modBuilder));
        }

        private static Type ArrayOrNewType(ITypeDescription typeInfo,
            IDictionary<ITypeDescription, Type> typeBuildMap,
            ModuleBuilder modBuilder)
        {
            return typeInfo.ArrayOf
                .Convert(elementType => BuildArrayType(typeBuildMap, elementType,typeInfo,modBuilder))
                .GetValue(() => AddNoNativeType(typeInfo, typeBuildMap, modBuilder));
        }

        private static Type AddNoNativeType(ITypeDescription typeInfo,
                                            IDictionary<ITypeDescription, Type> typeBuildMap,
                                            ModuleBuilder modBuilder)
        {
            var baseType = GetOrCreateType(typeBuildMap, modBuilder, typeInfo.BaseClass);
            var defineType = CreateType(modBuilder,
                typeInfo.TypeName.NameWithGenerics, baseType);
            typeBuildMap[typeInfo] = defineType;

            foreach (var field in typeInfo.Fields)
            {
                CreateFields(defineType, field, modBuilder, typeBuildMap);
            }
            return defineType;
        }

        private static Type AddNativeType(ITypeDescription typeInfo, Type type,
                                          IDictionary<ITypeDescription, Type> typeBuildMap)
        {
            typeBuildMap[typeInfo] = type;
            return type;
        }

        private static void CreateFields(TypeBuilder typeBuilder, SimpleFieldDescription field,
            ModuleBuilder modBuilder,
            IDictionary<ITypeDescription, Type> typeBuildMap)
        {
            var type = GetOrCreateType(typeBuildMap, modBuilder,field.Type);
            var generatedField = typeBuilder.DefineField(field.Name,
                                                         type,
                                                         FieldAttributes.Public);
            AddOptionalProperty(typeBuilder, field, type, generatedField);
        }

        private static void AddOptionalProperty(TypeBuilder typeBuilder,
            SimpleFieldDescription field,
            Type type, FieldBuilder generatedField)
        {
            if (char.IsLower(field.Name[0]) || char.IsSymbol(field.Name[0]))
            {
                CreateProperty(typeBuilder, field, type, generatedField);
            }
        }

        private static void CreateProperty(TypeBuilder typeBuilder, SimpleFieldDescription field,
            Type type, FieldBuilder generatedField)
        {
            var propertyName = field.AsPropertyName();
            var property = typeBuilder.DefineProperty(propertyName,
                                                      PropertyAttributes.HasDefault,
                                                      type, null);

            property.SetGetMethod(CreateGetter(typeBuilder, propertyName, generatedField));
            property.SetSetMethod(CreateSetter(typeBuilder, propertyName, generatedField));
        }

        private static MethodBuilder CreateGetter(TypeBuilder typeBuilder, string propertyName,
                                                  FieldBuilder generatedField)
        {
            var getterMethod = DefineMethod(typeBuilder,
                "get_" + propertyName, generatedField.FieldType, Type.EmptyTypes);

            var ilGenerator = getterMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, generatedField);
            ilGenerator.Emit(OpCodes.Ret);
            return getterMethod;
        }

        private static MethodBuilder CreateSetter(TypeBuilder typeBuilder, string propertyName,
                                                  FieldBuilder generatedField)
        {
            var setterMethod = DefineMethod(typeBuilder, "set_" + propertyName,null,new[] { generatedField.FieldType });

            var ilGenerator = setterMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, generatedField);
            ilGenerator.Emit(OpCodes.Ret);
            return setterMethod;
        }

        private static MethodBuilder DefineMethod(TypeBuilder typeBuilder,
            string methodName, Type returnType, Type[] parameterTypes)
        {
            return typeBuilder.DefineMethod(methodName,
                                            MethodAttributes.Public | MethodAttributes.SpecialName |
                                            MethodAttributes.HideBySig,
                                            returnType, parameterTypes);
        }

        private static Type BuildArrayType(IDictionary<ITypeDescription, Type> typeBuildMap,
            ITypeDescription elementDescription, ITypeDescription arrayType, ModuleBuilder modBuilder)
        {
            var elementType = typeBuildMap.TryGet(elementDescription)
                .GetValue(() => GetOrCreateType(typeBuildMap, modBuilder, elementDescription));
            var type = elementType.MakeArrayType();
            typeBuildMap[arrayType] = type;
            return type;
        }

        private static TypeBuilder CreateType(ModuleBuilder modBuilder,
            string className, Type baseType)
        {
            return modBuilder.DefineType(BuildName(className),
                                         TypeAttributes.Class | TypeAttributes.Public, baseType);
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
            return AppDomain.CurrentDomain
                .DefineDynamicAssembly(theName, AssemblyBuilderAccess.RunAndSave,
                                       Path.GetDirectoryName(theName.CodeBase));
        }
    }
}