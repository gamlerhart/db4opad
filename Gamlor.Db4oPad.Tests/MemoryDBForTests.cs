using System;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.IO;

namespace Gamlor.Db4oPad.Tests
{

    static class MemoryDBForTests
    {
        public static IObjectContainer NewDB()
        {
            return NewDB(cfg => { });
        }
        public static IObjectContainer NewDB(Action<IEmbeddedConfiguration> configurator)
        {
            var config = Db4oEmbedded.NewConfiguration();
            configurator(config);
            config.File.Storage = new MemoryStorage();
            return Db4oEmbedded.OpenFile(config, "!In:Memory!");
        }
    }
    class MultiContainerMemoryDB
    {
        private readonly IStorage storage = new MemoryStorage();

        public static MultiContainerMemoryDB Create()
        {
            return new MultiContainerMemoryDB();
        }

        public IObjectContainer NewDB()
        {
            return NewDB(cfg => { });
        }
        public IObjectContainer NewDB(Action<IEmbeddedConfiguration> configurator)
        {
            var config = Db4oEmbedded.NewConfiguration();
            config.File.Storage = storage;
            configurator(config);
            return Db4oEmbedded.OpenFile(config, "!In:Memory!");
        }
    }
}