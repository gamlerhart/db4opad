using System.Collections.Generic;

namespace Gamlor.Db4oPad.Tests.TestTypes
{

    class ClassWithoutFields
    {

    }

    class ClassWithFields
    {
        private string aField;
    }
    class RecursiveClass
    {
        private RecursiveClass aField;
    }
    class WithBuiltInGeneric
    {
        private List<string> aField;
    }
    class Generic<T>
    {
        private List<T> aField;
    }
    class Generic<T1, T2>
    {
        private Dictionary<T1, T2> aField;
    }
}