using System;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;

namespace Db4oPad.TestDBs
{
    class Program
    {
        static void Main(string[] args)
        {
            StoreExampleDBs();
            StoreMergedExampleDB();
            StoreWithVirtualFieds("virtualFields.db4o");
        }

        private static void StoreMergedExampleDB()
        {
            const string fileName ="allInOne.db4o";
            StoreLocationInfo(fileName);
            StorePreferencesInfo(fileName);
            StoreIndexExample(fileName);
            StoreNestedClassExample(fileName);
            StoreKnowGenericsWithUnknownParamterTypes(fileName);
            StoreDictionary(fileName);
            StoreWithNameCollisions(fileName);
            StoreSameNameDifferentAssemblies(fileName);
            StoreDBWithEnum(fileName);
        }

        private static void StoreExampleDBs()
        {
            StoreLocationInfo("databaseWithArrayFields.db4o");
            StorePreferencesInfo("databaseWithGenerics.db4o");
            StoreIndexExample("databaseWithIndexes.db4o");
            StoreNestedClassExample("databaseWithNestedClasses.db4o");
            StoreKnowGenericsWithUnknownParamterTypes("databaseWithKnownGenricsAndUnknowParameterTypes.db4o");
            StoreDictionary("databaseWithDictionary.db4o");
            StoreWithNameCollisions("dataspaceWithNameCollisions.db4o");
            StoreSameNameDifferentAssemblies("sameNameDifferentAssemblies.db4o");
            StoreDBWithEnum("databaseWithEnum.db4o");
        }

        private static void StoreDBWithEnum(string dbName)
        {
            using (var db = Db4oEmbedded.OpenFile(dbName))
            {
                db.Store(new ItemWithEnum());
                db.Store(new ItemWithEnum());
            }
        }

        private static void StoreSameNameDifferentAssemblies(string database)
        {
            using (var db = Db4oEmbedded.OpenFile(database))
            {
                db.Store(new SameNameInDifferentAssemblies());
                WriteIntoDB.Write(db);
            }
        }

        private static void StoreWithVirtualFieds(string databaseName)
        {
            var cfg = Db4oEmbedded.NewConfiguration();
            cfg.File.GenerateUUIDs = ConfigScope.Globally;
            cfg.File.GenerateCommitTimestamps = true;
            using (var db = Db4oEmbedded.OpenFile(cfg, databaseName))
            {
                StoreADemoDataModel(db);
            }
        }

        private static void StoreKnowGenericsWithUnknownParamterTypes(string databaseName)
        {
            using (var db = Db4oEmbedded.OpenFile(databaseName))
            {
                db.Store(new ListHolder());
            }

        }
        private static void StoreDictionary(string databaseName)
        {
            using (var db = Db4oEmbedded.OpenFile(databaseName))
            {
                db.Store(new DictionaryHolder());
            }

        }
        private static void StoreWithNameCollisions(string databaseName)
        {
            using (var db = Db4oEmbedded.OpenFile(databaseName))
            {
                db.Store(new SameNameInDifferentNamespaces());
                db.Store(new NameSpaceOne.SameNameInDifferentNamespaces());
                db.Store(new NameSpaceTwo.SameNameInDifferentNamespaces());
            }

        }

        private static void StorePreferencesInfo(string fileName)
        {
            using (var db = Db4oEmbedded.OpenFile(fileName))
            {
                var spagetti = new Food {Name = "Spagetti", Calories = 500};
                var pizza = new Food {Name = "Pizza", Calories = 700};

                var wine = new Drink {Name = "Wine", AlcoholVolumePercent = 12};
                var cola = new Drink {Name = "Cola", AlcoholVolumePercent = 0};

                var preferenceForSpagetti = new Preferences<Food> {For = spagetti, TheInfo = "Yummie"};
                var preferenceForPizza = new Preferences<Food> { For = pizza, TheInfo = "Also Yummie" };
                var preferenceForWine = new Preferences<Drink> { For = wine, TheInfo = "It depends" };
                var preferenceForCola = new Preferences<Drink> { For = cola, TheInfo = "There are better things" };

                db.Store(preferenceForSpagetti);
                db.Store(preferenceForPizza);
                db.Store(preferenceForWine);
                db.Store(preferenceForCola);

                var listOfThings = new ListOfThings();
                listOfThings.listOfDrinks.Add(cola);
                listOfThings.listOfFood.Add(pizza);
                db.Store(listOfThings);
            }
        }

        private static void StoreLocationInfo(string fileName)
        {
            using (var db = Db4oEmbedded.OpenFile(fileName))
            {
                StoreADemoDataModel(db);
            }
        }

        private static void StoreADemoDataModel(IObjectContainer db)
        {
            var mainLocation = new Location("MainLocation");
            mainLocation.AddAlternativeLocation(new Location("Alternative"));
            db.Store(mainLocation);
        }

        private static void StoreIndexExample(string fileName)
        {
            var cfg = Db4oEmbedded.NewConfiguration();
            cfg.Common.ObjectClass(typeof(IndexesOnFields)).ObjectField("firstIndexedField").Indexed(true);
            cfg.Common.ObjectClass(typeof(IndexesOnFields)).ObjectField("secondIndexedField").Indexed(true);
            using (var db = Db4oEmbedded.OpenFile(cfg,fileName))
            {
                db.Store(new IndexesOnFields());
                db.Store(new IndexesOnFields());
                db.Store(new IndexesOnFields());
                db.Store(new IndexesOnFields());
            }
        }

        private static void StoreNestedClassExample(string fileName)
        {
            using (var db = Db4oEmbedded.OpenFile(fileName))
            {
                db.Store(new NestedClasses.ChildClass("data"));
                db.Store(new NestedClasses());
            }
        }

    }

}
