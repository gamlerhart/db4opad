using System;
using System.Collections.Generic;

namespace Gamlor.Db4oPad
{
    /// <summary>
    /// Be aware that these implementations may
    /// depend on a valid <see cref="CurrentContext"/>
    /// </summary>
    public static class PadExtensions
    {
        public static readonly string NameSpace = typeof (PadExtensions).Namespace;

        public static IEnumerable<T> UpdateAll<T>(this IEnumerable<T> queryResult,Action<T> updateHandler)
            where T : class
        {
            foreach (var item in queryResult)
            {
                updateHandler(item);
                CurrentContext.GetCurrentContext().Store(item);
            }
            return queryResult;
        }   
    }
}