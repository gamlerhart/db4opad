using System;
using System.Collections.Generic;
using System.Linq;

namespace Gamlor.Db4oPad.Utils
{
    public static class MayExtensions
    {
        public static Maybe<T> AsMaybe<T>(this T refObject) where T : class
        {
            return null == refObject ? Maybe<T>.Empty : Maybe.From(refObject);
        }

        public static Maybe<T> FirstMaybe<T>(this IEnumerable<T> collection) where T : class
        {
            return collection.FirstOrDefault().AsMaybe();
        }

        public static Maybe<TResult> MaybeCast<TResult>(this object objectToCast)
        {
            if (objectToCast is TResult)
            {
                return (TResult)objectToCast;
            }
            else
            {
                return Maybe<TResult>.Empty;
            }
        }

        public static Maybe<TValue> TryGet<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            if (null == dic)
            {
                throw new ArgumentNullException("dic");
            }
            TValue result;
            return dic.TryGetValue(key, out result) ? result : Maybe<TValue>.Empty;
        }
    }

}