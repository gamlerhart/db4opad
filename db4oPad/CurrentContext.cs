using System;
using System.Threading;

namespace Gamlor.Db4oPad
{
    static class CurrentContext
    {
        private static readonly ThreadLocal<DatabaseContext> context = new ThreadLocal<DatabaseContext>();
        public static DatabaseContext GetCurrentContext()
        {
            if(context.IsValueCreated && context.Value!=null)
            {
                return context.Value;
            }
            throw new InvalidOperationException("No context available");
        }

        public static void NewContext(DatabaseContext newContext)
        {
            context.Value = newContext;
        }

        public static void CloseContext()
        {
            context.Value = null;
        }
    }
}