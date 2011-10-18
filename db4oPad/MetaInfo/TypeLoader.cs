using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal delegate Maybe<Type> TypeResolver(TypeName toFind);

    /// <summary>
    /// Loads types from the optional specified assemblies.
    /// </summary>
    internal class TypeLoader
    {
        private readonly IEnumerable<string> filePaths;
        private TypeLoader(IEnumerable<string> filePaths)
        {
            new{filePaths}.CheckNotNull();
            this.filePaths = filePaths.ToList();
        }

        public static TypeResolver Create(IEnumerable<string> filePaths)
        {
            return new TypeLoader(filePaths.ToList()).Resolver();
        }
        private TypeResolver Resolver()
        {
            return Resolve;
        }

        private Maybe<Type> Resolve(TypeName toFind)
        {
            return FindInCurrentAppDomain(toFind)
                .Otherwise(()=>FindInGivenAssemblyPaths(toFind));
        }

        private Maybe<Type> FindInGivenAssemblyPaths(TypeName toFind)
        {
            foreach (var assemblyPath in filePaths)
            {
                var type = TryLoad(assemblyPath).Combine(a => LoadTypeFromAssembly(a,toFind));
                if (type.HasValue)
                {
                    return type;
                }
            }
            return Maybe<Type>.Empty;
        }

        private static Maybe<Assembly> TryLoad(string assemblyPath)
        {
            if(File.Exists(assemblyPath))
            {
                return TryLoadAssembly(assemblyPath);     
            }
            return Maybe<Assembly>.Empty;
        }

        private static Maybe<Assembly> TryLoadAssembly(string assemblyPath)
        {
            try
            {
                return Maybe.From(Assembly.LoadFrom(assemblyPath));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Trace.Write(e.StackTrace);
                return Maybe<Assembly>.Empty;
            }
        }

        private Maybe<Type> FindInCurrentAppDomain(TypeName toFind)
        {
            var assembly = from a in AppDomain.CurrentDomain.GetAssemblies()
                           where a.GetName().Name == toFind.AssemblyName
                           select a;
            return assembly.FirstMaybe()
                .Combine(a => LoadTypeFromAssembly(a,toFind));
        }

        private Maybe<Type> LoadTypeFromAssembly(Assembly assembly, TypeName toFind)
        {
            var name = Generify(toFind);
            return assembly.GetType(name).AsMaybe().Combine(t=>InstantiateGeneric(t,toFind));
        }

        private Maybe<Type> InstantiateGeneric(Type type, TypeName toFind)
        {
            return IsGeneric(toFind) ? TryCreateGenericInstance(toFind, type) : type;
        }

        private Maybe<Type> TryCreateGenericInstance(TypeName toFind, Type type)
        {
            if(toFind.GenericArguments.Any(t=>!t.HasValue))
            {
                return type;
            }
            var types = toFind.GenericArguments.Select(t=>Resolve(t.Value));
            if(types.All(t=>t.HasValue))
            {
                return type.MakeGenericType(types.Select(t=>t.Value).ToArray());
            }
            return Maybe<Type>.Empty;
        }

        private static string Generify(TypeName toFind)
        {
            return toFind.NameAndNamespace;
        }

        private static bool IsGeneric(TypeName toFind)
        {
            return toFind.GenericArguments.Any();
        }

        internal static TypeResolver DefaultTypeResolver()
        {
            var loader = Create(new string[0]);
            return loader;

        }
    }
}