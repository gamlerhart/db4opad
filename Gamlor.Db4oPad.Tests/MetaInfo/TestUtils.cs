using System;
using System.IO;
using System.Reflection;

namespace Gamlor.Db4oPad.Tests.MetaInfo
{
    public static class TestUtils
    {
        public static void WithTestContext(Action action)
        {
            var db = MemoryDBForTests.NewDB();
            using(var ctx = DatabaseContext.Create(db,new AssemblyName("test")
                                                          {
                                                              CodeBase = Path.GetTempFileName()
                                                          }))
            {
                CurrentContext.NewContext(ctx);
                action();
            }
        }
    }
}