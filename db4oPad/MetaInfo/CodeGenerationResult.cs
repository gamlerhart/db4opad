using System;
using System.Collections;
using System.Collections.Generic;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class CodeGenerationResult : IEnumerable<KeyValuePair<ITypeDescription, Type>>
    {
        private readonly IDictionary<ITypeDescription, Type> types;
        private readonly Type dataContext;


        public CodeGenerationResult(Type dataContext,IDictionary<ITypeDescription, Type> types)
        {
            this.dataContext = dataContext;
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

        public IDictionary<ITypeDescription, Type> Types
        {
            get { return types; }
        }

        public Type DataContext
        {
            get { return dataContext; }
        }
    }
}