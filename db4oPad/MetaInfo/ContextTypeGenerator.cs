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

    internal class ContextTypeGenerator
    {
        private const string MetaDataProperty = "MetaData";
        public const string QueryContextClassName = "LINQPad.User.TypedDataContext";
        private const string MetaDataNameSpace= "LINQPad.User.MetaData";
        private const string MetaDataClassName = MetaDataNameSpace + ".Repo";

        private readonly TypeBuilder rootType;
        private readonly ModuleBuilder moduleBuilder;

        public ContextTypeGenerator(ModuleBuilder moduleBuilder, TypeBuilder rootType)
        {
            this.rootType = rootType;
            this.moduleBuilder = moduleBuilder;
        }

        internal static Type CreateContextType(ModuleBuilder builder,
            IEnumerable<KeyValuePair<ITypeDescription, Type>> types)
        {
            var typesGroupesByName = from t in types
                                     where t.Key.IsBusinessEntity
                                     group t by t.Key.Name
                                     into byName
                                         select new ByNameGrouping(byName.Key,byName);
            var typeBuilder = builder.DefineType(QueryContextClassName,
                                                 CodeGenerationUtils.PublicClass(), typeof(ContextRoot));
            return new ContextTypeGenerator(builder,typeBuilder).Build(typesGroupesByName);
        }

        private Type Build(IEnumerable<ByNameGrouping> typesGroupesByName)
        {
            CreateQueryEntryPoints(typesGroupesByName);
            CreateMetaDataStructure(typesGroupesByName);
            return rootType.CreateType();
        }

        private TypeBuilder CreateQueryEntryPoints(IEnumerable<ByNameGrouping> typesGroupesByName)
        {
            return new NamespaceContextGenerator("Query",moduleBuilder, rootType, typesGroupesByName,
                                          CreateQueryGetter,
                                          (t,d) => typeof(ExtendedQueryable<>).MakeGenericType(t)).BuildType();
        }

        private void CreateMetaDataStructure(IEnumerable<ByNameGrouping> typesGroupesByName)
        {
            var metaInfoType = CreateMetaInfoProperty(typesGroupesByName);
            var constructorOfMetaInfoType = metaInfoType.GetConstructors().Single();

            var property = CodeGenerationUtils.DefineProperty(rootType, MetaDataProperty, metaInfoType);

            var getterMethod = CodeGenerationUtils.GetterMethodFor(rootType, property, CodeGenerationUtils.StaticPublicGetter());
            var lambda = Expression.Lambda(typeof(Func<>).MakeGenericType(metaInfoType),
                                           Expression.New(constructorOfMetaInfoType), new ParameterExpression[0]);
            lambda.CompileToMethod(getterMethod);
        }

        private Type CreateMetaInfoProperty(IEnumerable<ByNameGrouping> typesGroupesByName)
        {
            var typeBuilder = moduleBuilder.DefineType(MetaDataClassName, CodeGenerationUtils.PublicClass());

            var finalType = new NamespaceContextGenerator("MetaData",moduleBuilder, typeBuilder, typesGroupesByName,
                                              CreateMetaDataGetter,
                                              (t,d) => BuildMetaInfoType(moduleBuilder,d)).BuildType();
           return finalType.CreateType();
        }

        private void CreateMetaDataGetter(MethodBuilder getterMethod,
                                                       Type type,
                                                       ITypeDescription typeDescription)
        {
            CodeGenerationUtils.ReturnNewInstanceILInstructions(
                type.GetConstructors().Single(), getterMethod);
        }



        private static Type BuildMetaInfoType(ModuleBuilder moduleBuilder, ITypeDescription type)
        {
            var typeBuilder = moduleBuilder.DefineType(MetaDataNameSpace + "." + type.TypeName.NameWithGenerics, CodeGenerationUtils.PublicClass());
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
            var property = CodeGenerationUtils.DefineProperty(typeBuilder, propertyName, typeof(string));
            var methodBuilder = CodeGenerationUtils.GetterMethodFor(typeBuilder, property, CodeGenerationUtils.PublicGetter());
            
            var ilCode = methodBuilder.GetILGenerator();
            ilCode.Emit(OpCodes.Ldstr, valueToReturn);
            ilCode.Emit(OpCodes.Ret);
        }

        private static void CreateQueryGetter(MethodBuilder getterMethod,
                                                       Type type,
                                                       ITypeDescription typeDescription)
        {
            var ilGenerator = getterMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Call, StaticQueryCall(type));
            ilGenerator.Emit(OpCodes.Ret);
        }


        private static MethodInfo StaticQueryCall(Type forType)
        {
            return typeof(CurrentContext).GetMethod("Query").MakeGenericMethod(forType);
        }
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