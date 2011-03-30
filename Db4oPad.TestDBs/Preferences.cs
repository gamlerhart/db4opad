using System.Collections.Generic;

namespace Db4oPad.TestDBs
{
    public class Preferences<T>
    {
        public T For { get; set; }
        public string TheInfo { get; set; }
    }
    public abstract class Named
    {
        public string Name{get;set;}
    }

    public class Food : Named
    {
        public int Calories { get; set; }
    }
    public class Drink :Named
    {
        public int AlcoholVolumePercent { get; set; }

    }
    public class ListOfThings
    {
        public List<Food> listOfFood = new List<Food>();
        public List<Drink> listOfDrinks = new List<Drink>();
    }
}