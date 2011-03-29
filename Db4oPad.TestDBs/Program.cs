using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Db4objects.Db4o;

namespace Db4oPad.TestDBs
{
    class Program
    {
        static void Main(string[] args)
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
