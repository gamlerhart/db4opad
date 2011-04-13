using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gamlor.Db4oPad
{
    public static class ExtendedQueryable
    {
        public static ExtendedQueryable<T> Create<T>(IQueryable<T> implementation)
        {
            return new ExtendedQueryable<T>(implementation);
        }     
    }

    public class ExtendedQueryable<T> : IQueryable<T>
    {
        private readonly IQueryable<T> innerImplementation;

        public ExtendedQueryable(IQueryable<T> innerImplementation)
        {
            this.innerImplementation = innerImplementation;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return innerImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression
        {
            get { return innerImplementation.Expression; }
        }

        public Type ElementType
        {
            get { return innerImplementation.ElementType; }
        }

        public IQueryProvider Provider
        {
            get { return innerImplementation.Provider; }
        }

        public T New(params object[] arguments)
        {
            if(arguments.Any(a=>a==null))
            {
                throw new ArgumentException("Currently we cannot support null as an argument");
            }
            var typeOfArguments = (from arg in arguments
                                  select arg.GetType()).ToArray();
            var constructor = typeof(T).GetConstructor(typeOfArguments);
            if(null==constructor)
            {
                throw new ArgumentException(string.Format("Couldn't find a constructor for {0} with the arguments {1}", typeof(T), arguments));
            }
            return (T)constructor.Invoke(arguments);
        }
    }
}