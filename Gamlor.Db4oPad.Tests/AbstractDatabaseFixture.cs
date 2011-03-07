using System;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    public abstract class AbstractDatabaseFixture
    {
        private IObjectContainer db;
        private MultiContainerMemoryDB fileSystem;

        [SetUp]
        public void Setup()
        {
            this.fileSystem = MultiContainerMemoryDB.Create();
            this.db = fileSystem.NewDB(ConfigureDb);
            FixtureSetup(db);
        }

        protected virtual void FixtureSetup(IObjectContainer db)
        {
        }

        protected virtual void ConfigureDb(IEmbeddedConfiguration configuration)
        {

        }
        public IObjectContainer Reopen()
        {
            db.Close();
            db = fileSystem.NewDB();
            return db;
        }

        public IObjectContainer Reopen(Action<IEmbeddedConfiguration> configuration)
        {
            db.Close();
            db = fileSystem.NewDB(configuration);
            return db;
        }

        public IObjectContainer DB
        {
            get { return db; }
        }
    }

}