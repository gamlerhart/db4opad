using System;
using Db4objects.Db4o;

namespace Db4oPad.TestDBs
{
    class Program
    {
        static void Main(string[] args)
        {
            StoreLocationInfo();
            StorePreferencesInfo();
        }

        private static void StorePreferencesInfo()
        {
            using (var db = Db4oEmbedded.OpenFile("databaseWithGenerics.db4o"))
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

        private static void StoreLocationInfo()
        {
            using (var db = Db4oEmbedded.OpenFile("databaseWithArrayFields.db4o"))
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
    }
}
