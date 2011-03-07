using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Gamlor.Db4oPad.Utils
{
    public static class ValidationExtensions
    {
        public static void CheckNotNull<T>(this T container) where T : class
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            NullChecker<T>.Check(container);
        }

        private static class NullChecker<T> where T : class
        {
            private static readonly List<Tuple<Func<T, bool>, string>> checkers;
            static NullChecker()
            {
                checkers = new List<Tuple<Func<T, bool>, string>>();
                foreach (var property in typeof(T).GetProperties())
                {
                    AddChecker(property);
                }
            }

            private static void AddChecker(PropertyInfo property)
            {
                if (CanBeNull(property))
                {
                    ParameterExpression param = Expression.Parameter(typeof(T), "container");
                    Expression propertyAccess = Expression.Property(param, property);
                    Expression nullValue = Expression.Constant(null, property.PropertyType);
                    Expression equality = Expression.Equal(propertyAccess, nullValue);
                    var lambda = Expression.Lambda<Func<T, bool>>(equality, param);
                    checkers.Add(Tuple.Create(lambda.Compile(), property.Name));
                }
            }

            internal static void Check(T item)
            {
                foreach (var t in checkers)
                {
                    if (t.Item1(item))
                    {
                        throw new ArgumentNullException(t.Item2);
                    }
                }
            }

            private static bool CanBeNull(PropertyInfo property)
            {
                return !property.PropertyType.IsValueType;
            }
        }
    }

}