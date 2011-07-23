namespace Db4oPad.TestDBs
{
    public class SameNameInDifferentNamespaces
    {
        
    }

    namespace NameSpaceOne
    {
        public class SameNameInDifferentNamespaces
        {
            private string field1 = "";
        }
    
    }
    namespace NameSpaceTwo
    {
        public class SameNameInDifferentNamespaces
        {
            private string field2 = "";
        }

    }
}