using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal static class CodeGenerator
    {
        public const string NameSpace = "LINQPad.User";
        public const int EnumRange = 4096;

        internal static CodeGenerationResult Create(IEnumerable<ITypeDescription> metaInfo,
            AssemblyName intoAssembly)
        {
            var assemblyBuilder = CreateAssembly(intoAssembly);
            var builder = CreateModule(assemblyBuilder);
            var types = CreateTypes(builder, metaInfo);
            var contextType = ContextTypeGenerator.CreateContextType(builder, types);
            assemblyBuilder.Save(Path.GetFileName(intoAssembly.CodeBase));
            return new CodeGenerationResult(contextType, types);
        }

        public static CodeGenerationResult Create(IEnumerable<ITypeDescription> metaInfo, Assembly candidateAssembly)
        {
            var types = metaInfo.ToDictionary(mi => mi, mi => FindType(mi,candidateAssembly));
            var contextType = candidateAssembly.GetType(ContextTypeGenerator.QueryContextClassName);
            return new CodeGenerationResult(contextType, types);
        }

        private static Type FindType(ITypeDescription typeInfo, Assembly candidateAssembly)
        {
            return typeInfo.TryResolveType(n => FindType(n, candidateAssembly))
                .Convert(t => t)
                .GetValue(
                () => FindTypeOrArray(typeInfo, candidateAssembly));
        }

        private static Type FindTypeOrArray(ITypeDescription typeInfo, Assembly candidateAssembly)
        {
            return typeInfo.TryResolveType(t =>FindType(t,candidateAssembly))
                .GetValue(() => candidateAssembly.GetType(BuildName(typeInfo.TypeName)));
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
            return typeInfo.TryResolveType(t=>GetOrCreateType(typeBuildMap, modBuilder, t))
                .Convert(t => AddNativeType(typeInfo, t, typeBuildMap))
                .GetValue(() => AddNoNativeType(typeInfo, typeBuildMap, modBuilder));
        }

        private static Type AddNoNativeType(ITypeDescription typeInfo,
                                            IDictionary<ITypeDescription, Type> typeBuildMap,
                                            ModuleBuilder modBuilder)
        {
            var baseType = typeInfo.BaseClass.Convert(bc=>GetOrCreateType(typeBuildMap, modBuilder,bc ));
            if(baseType==typeof(Enum))
            {
                return BuildEnum(typeInfo,typeBuildMap, modBuilder);
            } else
            {
                var defineType = CreateType(modBuilder,
                    typeInfo.TypeName, baseType);
                typeBuildMap[typeInfo] = defineType;

                foreach (var field in typeInfo.Fields)
                {
                    CreateFields(defineType, field, modBuilder, typeBuildMap);
                }
                return defineType;
            }
        }

        private static Type BuildEnum(ITypeDescription typeInfo, IDictionary<ITypeDescription, Type> typeBuildMap, ModuleBuilder modBuilder)
        {
            var enumBuilder  = modBuilder.DefineEnum(BuildName(typeInfo.TypeName), TypeAttributes.Class | TypeAttributes.Public,
                                  typeof(int));
            typeBuildMap[typeInfo] = enumBuilder;
            for (int i = -EnumRange; i < EnumRange; i++)
            {
                if(i<0)
                {
                    enumBuilder.DefineLiteral("Value_Negative" + Math.Abs(i),i);
                }  else
                {
                    enumBuilder.DefineLiteral("Value_" + Math.Abs(i), i);
                } 
            }
            return enumBuilder.CreateType();
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
            Type type, FieldInfo generatedField)
        {
            if (char.IsLower(field.Name[0]) || char.IsSymbol(field.Name[0]))
            {
                CreateProperty(typeBuilder, field, type, generatedField);
            }
        }

        private static void CreateProperty(TypeBuilder typeBuilder, SimpleFieldDescription field,
            Type type, FieldInfo generatedField)
        {
            var propertyName = field.AsPropertyName();
            var property = typeBuilder.DefineProperty(propertyName,
                                                      PropertyAttributes.HasDefault,
                                                      type, null);

            property.SetGetMethod(CreateGetter(typeBuilder, propertyName, generatedField));
            property.SetSetMethod(CreateSetter(typeBuilder, propertyName, generatedField));
        }

        private static MethodBuilder CreateGetter(TypeBuilder typeBuilder, string propertyName,
                                                  FieldInfo generatedField)
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
                                                  FieldInfo generatedField)
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

        private static TypeBuilder CreateType(ModuleBuilder modBuilder,
            TypeName className, Maybe<Type> baseType)
        {
            return modBuilder.DefineType(BuildName(className),
                                         TypeAttributes.Class | TypeAttributes.Public, baseType.GetValue(typeof(object)));
        }

        private static string BuildName(TypeName className)
        {
            return NameSpace + "." + CodeGenerationUtils.ClassName(className);
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