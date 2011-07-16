using System.Collections.Generic;

namespace Db4oPad.TestDBs
{
    public class ListHolder
    {
        public ListHolder()
        {
            ListOfItems = new List<ListItem>()
                              {
                                  new ListItem()
                              };
            ListOfNotStoredObjects = new List<NoInstancesOfThisAreStored>();
        }

        public IList<ListItem> ListOfItems { get; set; }
        public IList<NoInstancesOfThisAreStored> ListOfNotStoredObjects { get; set; }
        public List<INotStoredItem> ListOfInterfaces { get; set; }
    }

    public class ListItem{}
    public class NoInstancesOfThisAreStored{}
    public interface INotStoredItem{}

}