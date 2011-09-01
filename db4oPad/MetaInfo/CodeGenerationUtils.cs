using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Gamlor.Db4oPad.MetaInfo
{
    static class CodeGenerationUtils
    {
        internal static MethodBuilder GetterMethodFor(TypeBuilder typeBuilder,
                                                      PropertyBuilder property, MethodAttributes access)
        {
            var getterMethod = typeBuilder.DefineMethod("get_" + property.Name,
                                                        access, property.PropertyType, null);

            property.SetGetMethod(getterMethod);
            return getterMethod;
        }


        internal static void ReturnNewInstanceILInstructions(ConstructorInfo constructor, MethodBuilder getterMethod)
        {
            var ilGenerator = getterMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Newobj, constructor);
            ilGenerator.Emit(OpCodes.Ret);
        }


        internal static PropertyBuilder DefineProperty(TypeBuilder typeBuilder,
                                                       string name, Type type)
        {
            return typeBuilder.DefineProperty(name,
                                              PropertyAttributes.HasDefault,
                                              type, null);
        }

        internal static TypeAttributes PublicClass()
        {
            return TypeAttributes.Class | TypeAttributes.Public;
        }

        internal static MethodAttributes StaticPublicGetter()
        {
            return MethodAttributes.Public | MethodAttributes.SpecialName |
                   MethodAttributes.HideBySig | MethodAttributes.Static;
        }

        internal static MethodAttributes PublicGetter()
        {
            return MethodAttributes.Public | MethodAttributes.SpecialName |
                   MethodAttributes.HideBySig;
        }

        internal static MethodAttributes PublicConstructurSignature()
        {
            return MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        }
    }
}