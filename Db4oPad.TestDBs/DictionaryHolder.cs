using System.Collections.Generic;

namespace Db4oPad.TestDBs
{
    public class DictionaryHolder
    {
        public DictionaryHolder()
        {
            Items = new Dictionary<string, DictionaryItem>();
        }

        public IDictionary<string, DictionaryItem> Items { get; set; } 
    }

    public class DictionaryItem{}
}