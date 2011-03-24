using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamlor.Db4oPad.MetaInfo
{
    /// <summary>
    /// Loads types from the optional specified assemblies.
    /// </summary>
    internal class TypeLoader
    {
        private readonly TypeResolver nativeResolver = MetaDataReader.DefaultTypeResolver();
        private TypeLoader(IEnumerable<string> filePaths)
        {
        }

        public static TypeResolver Create(IEnumerable<string> filePaths)
        {
            return new TypeLoader(filePaths.ToList()).Resolver();
        }

        private TypeResolver Resolver()
        {
            return nativeResolver;
        }
    }
}