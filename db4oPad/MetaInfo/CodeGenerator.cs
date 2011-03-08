using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class CodeGenerator
    {
        public const String NameSpace = "LINQPad.User";
        internal static Result Create(IEnumerable<ITypeDescription> metaInfo,
            AssemblyName intoAssembly)
        {
            var assemblyBuilder = CreateAssembly(intoAssembly);
            var builder = CreateModule(assemblyBuilder);
            var dictionary = metaInfo.ToDictionary(mi => mi, mi => Maybe<Type>.Empty);
            var types = CreateTypes(builder, dictionary);
            assemblyBuilder.Save(Path.GetFileName(intoAssembly.CodeBase));
            return new Result(types);
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
            return modBuilder.DefineType(NameSpace + "." + className,
                                         TypeAttributes.Class | TypeAttributes.Public);
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

        public class Result : IEnumerable<KeyValuePair<ITypeDescription, Type>>
        {
            private readonly IDictionary<ITypeDescription, Type> types;


            public Result(IDictionary<ITypeDescription, Type> types)
            {
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
        }
    }

}