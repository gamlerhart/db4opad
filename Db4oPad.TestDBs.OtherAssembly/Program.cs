using Db4objects.Db4o;

namespace Db4oPad.TestDBs
{
    public class SameNameInDifferentAssemblies
    {

    }

    public static class WriteIntoDB
    {
        public static void Write(IObjectContainer container)
        {
            container.Store(new SameNameInDifferentAssemblies());   
        }
    }
}