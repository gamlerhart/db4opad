using System;
using Db4objects.Db4o;

namespace Gamlor.Db4oPad.ExternalAssemblyForTests
{
    public class AType
    {
        
    }
    public class AGeneric<T>
    {

    }

    class Program
    {
        public static void Main(string[] args)
        {
            using (var db = Db4oEmbedded.OpenFile("withKnownTypes.db4o"))
            {
                db.Store(new AType());
            }   
        }
    }

}
