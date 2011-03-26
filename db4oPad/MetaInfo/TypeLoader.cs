using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    /// <summary>
    /// Loads types from the optional specified assemblies.
    /// </summary>
    internal class TypeLoader
    {
        private readonly IEnumerable<string> filePaths;
        private readonly TypeResolver nativeResolver = MetaDataReader.DefaultTypeResolver();
        private TypeLoader(IEnumerable<string> filePaths)
        {
            new{filePaths}.CheckNotNull();
            this.filePaths = filePaths.ToList();
        }
        /// <summary>
        /// Assumes that the different assemblies are seperated by a new line
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        public static TypeResolver Create(string filePaths)
        {
            return new TypeLoader(ParseFilePatsh(filePaths)).Resolver();
        }

        private static IEnumerable<string> ParseFilePatsh(string filePaths)
        {
            return filePaths.Split(Environment.NewLine.ToCharArray());
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
            return nativeResolver(toFind)
                .Otherwise(()=>FindInCurrentAppDomain(toFind))
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

        private Maybe<Assembly> TryLoad(string assemblyPath)
        {
            if(File.Exists(assemblyPath))
            {
                return TryLoadAssembly(assemblyPath);     
            }
            return Maybe<Assembly>.Empty;
        }

        private Maybe<Assembly> TryLoadAssembly(string assemblyPath)
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
            return assembly.GetType(name).AsMaybe().Convert(t=>InstantiateGeneric(t,toFind));
        }

        private Type InstantiateGeneric(Type type, TypeName toFind)
        {
            if (IsGeneric(toFind))
            {
                var types = toFind.GenericArguments.Select(n => Resolve(n).Value);
                return type.MakeGenericType(types.ToArray());
            }
            return type;
        }

        private static string Generify(TypeName toFind)
        {
            return toFind.NameAndNamespace + GenericNumber(toFind);
        }

        private static string GenericNumber(TypeName toFind)
        {
            if(IsGeneric(toFind))
            {
                return "`" + toFind.GenericArguments.Count();
            }
            return "";
        }

        private static bool IsGeneric(TypeName toFind)
        {
            return toFind.GenericArguments.Any();
        }
    }
}