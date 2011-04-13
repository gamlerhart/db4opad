using System;
using System.Linq;
using System.Threading;

namespace Gamlor.Db4oPad
{
    public static class CurrentContext
    {
        private static readonly ThreadLocal<DatabaseContext> context = new ThreadLocal<DatabaseContext>();
        internal static DatabaseContext GetCurrentContext()
        {
            if(IsAvailable())
            {
                return context.Value;
            }
            throw new InvalidOperationException("No context available");
        }

        internal static void NewContext(DatabaseContext newContext)
        {
            context.Value = newContext;
        }

        public static void CloseContext()
        {
            if (IsAvailable())
            {
                context.Value.Dispose();
            }
            context.Value = null;
        }

        public static ExtendedQueryable<T> Query<T>()
        {
            return GetCurrentContext().Query<T>();
        }

        private static bool IsAvailable()
        {
            return context.IsValueCreated && context.Value != null;
        }
    }
}