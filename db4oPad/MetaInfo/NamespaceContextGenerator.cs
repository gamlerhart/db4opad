using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class NamespaceContextGenerator
    {
        private readonly IDictionary<string, TypeBuilder> nameSpaceHolder = new Dictionary<string, TypeBuilder>();
        private readonly ModuleBuilder moduleBuilder;
        private readonly TypeBuilder rootType;
        private readonly string nameSpaceClassesPrefix;
        private readonly IEnumerable<ByNameGrouping> nameGroupsToBuild;
        private readonly Action<MethodBuilder, Type, ITypeDescription> buildPropertyOnType;
        private readonly Func<Type,ITypeDescription,Type> propertyTypeBuilder;

        internal NamespaceContextGenerator(string nameSpaceClassesPrefix, ModuleBuilder moduleBuilder,
                                           TypeBuilder rootType, IEnumerable<ByNameGrouping> nameGroupsToBuild,
                                           Action<MethodBuilder, Type, ITypeDescription> buildPropertyOnType,
                                        Func<Type, ITypeDescription, Type> propertyTypeBuilder)
        {
            this.nameSpaceClassesPrefix = nameSpaceClassesPrefix;
            this.moduleBuilder = moduleBuilder;
            this.rootType = rootType;
            this.nameGroupsToBuild = nameGroupsToBuild;
            this.buildPropertyOnType = buildPropertyOnType;
            this.propertyTypeBuilder = propertyTypeBuilder;
        }

        public TypeBuilder BuildType()
        {
            foreach (var type in nameGroupsToBuild)
            {
                CreateEntries(type);
            }
            FinishNamespaces();
            return rootType;
        }

        private void CreateEntries(ByNameGrouping type)
        {
            foreach (var typeToBuildFor in type.Members)
            {
                var definePropertyOn = FindLocationForProperty(type, typeToBuildFor.Key);
                var querableType = propertyTypeBuilder(typeToBuildFor.Value,typeToBuildFor.Key);
//                var property = CodeGenerationUtils.DefineProperty(definePropertyOn, typeToBuildFor.Key.Name, querableType);
//                var getterMethod = CodeGenerationUtils.GetterMethodFor(definePropertyOn, property, QueryPropertyAccessRights(definePropertyOn));
//                buildPropertyOnType(getterMethod, typeToBuildFor.Value,
//                    typeToBuildFor.Key);
            }
        }

        private TypeBuilder FindLocationForProperty(ByNameGrouping type,
                                                    ITypeDescription forType)
        {
            if (type.HasMultipleValues)
            {
                var typeNameSeparation = forType.TypeName.NameAndNamespace.LastIndexOf('.');
                var namespaceName = forType.TypeName.NameAndNamespace.Substring(0, typeNameSeparation);
                return GetOrCreateNamespaceFor(namespaceName);
            }
            return rootType;
        }


        private void FinishNamespaces()
        {
            foreach (var typeBuilder in nameSpaceHolder.Values)
            {
                typeBuilder.CreateType();
            }
        }

        private TypeBuilder GetOrCreateNamespaceFor(string namespaceName)
        {
            var currentNamespace = "";
            foreach (var ns in namespaceName.Split('.'))
            {
                var lastNameSpace = currentNamespace;
                currentNamespace = currentNamespace + (currentNamespace.Length != 0 ? "." : "") + ns;
                var clsFixNS = currentNamespace;
                this.nameSpaceHolder.TryGet(currentNamespace)
                    .GetValue(() => BuildNamespaceContext(lastNameSpace, clsFixNS, ns));
            }
            return nameSpaceHolder[currentNamespace];
        }   



        static MethodBuilder CreateNewInstanceGetter(TypeBuilder typeBuilder, PropertyBuilder property, 
            ConstructorInfo constructor, MethodAttributes accessRights)
        {
            var getterMethod = CodeGenerationUtils.GetterMethodFor(typeBuilder, property, accessRights);

            CodeGenerationUtils.ReturnNewInstanceILInstructions(constructor, getterMethod);
            return getterMethod;
        }

        private TypeBuilder BuildNamespaceContext(string lastNameSpace,
                                                  string forNamespace, string ns)
        {
            var currentType = moduleBuilder.DefineType(CodeGenerator.NameSpace + "." + forNamespace + "."+nameSpaceClassesPrefix+"NameSpaceContext",
                                                       CodeGenerationUtils.PublicClass());
            var parentNS = nameSpaceHolder.TryGet(lastNameSpace).GetValue(rootType);

            var accessType = QueryPropertyAccessRights(parentNS);

            var property = CodeGenerationUtils.DefineProperty(parentNS, ns, currentType);
            var constructor = currentType.DefineConstructor(CodeGenerationUtils.PublicConstructurSignature(), CallingConventions.Standard, new Type[0]);
            var ilGenerator = constructor.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, typeof(object).GetConstructors().Single());
            ilGenerator.Emit(OpCodes.Ret);
            property.SetGetMethod(CreateNewInstanceGetter(currentType, property,constructor, accessType));

            nameSpaceHolder[forNamespace] = currentType;
            return currentType;
        }

        private MethodAttributes QueryPropertyAccessRights(TypeBuilder parentNS)
        {
            return parentNS == rootType ? CodeGenerationUtils.StaticPublicGetter() : CodeGenerationUtils.PublicGetter();
        }
    }
}