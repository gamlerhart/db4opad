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

        private static Maybe<Type> FindInCurrentAppDomain(TypeName toFind)
        {
            var assembly = from a in AppDomain.CurrentDomain.GetAssemblies()
                           where a.GetName().Name == toFind.AssemblyName
                           select a;
            return assembly.FirstMaybe()
                .Combine(a => LoadTypeFromAssembly(a,toFind));
        }

        private static Maybe<Type> LoadTypeFromAssembly(Assembly assembly, TypeName toFind)
        {
            var name = toFind.NameAndNamespace;
            return assembly.GetType(name).AsMaybe();
        }
    }
}