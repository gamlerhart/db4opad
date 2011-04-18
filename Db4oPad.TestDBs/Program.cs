using System;
using Db4objects.Db4o;

namespace Db4oPad.TestDBs
{
    class Program
    {
        static void Main(string[] args)
        {
            StoreExampleDBs();
            StoreMergedExampleDB();
        }

        private static void StoreMergedExampleDB()
        {
            const string fileName ="allInOne.db4o";
            StoreLocationInfo(fileName);
            StorePreferencesInfo(fileName);
            StoreIndexExample(fileName);
        }

        private static void StoreExampleDBs()
        {
            StoreLocationInfo("databaseWithArrayFields.db4o");
            StorePreferencesInfo("databaseWithGenerics.db4o");
            StoreIndexExample("databaseWithIndexes.db4o");
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

    }

}
