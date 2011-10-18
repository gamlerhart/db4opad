using System;
using System.Collections.Generic;

namespace Gamlor.Db4oPad.Tests.TestTypes
{

    // Here we're listing test-types. We're not using it. Therefore we can suppress the warnings.
#pragma warning disable 169
    // ReSharper disable ConvertToAutoProperty
    class ClassWithoutFields
    {

    }

    class ClassWithFields
    {
        public string aField;
    }
    class ClassWithIndexedFields
    {
        private string indexedField;
        private string notIndexedField;
    }
    class ClassWithHalfKnownGeneric
    {
        private IList<ListItem> theList = new List<ListItem>();
        private List<IUnknownType> anotherList;
    }
    class ListItem{}

    internal interface IUnknownType
    {
        
    } 
    class RecursiveClass
    {
        public RecursiveClass aField;
    }

    class WithBuiltInGeneric
    {
        private List<string> aField;

        public List<string> AField
        {
            get { return aField; }
            set { aField = value; }
        }
    }
    class WithMixedGeneric
    {
        private List<ClassWithoutFields> aField;
    }
    class Generic<T>
    {
        private List<T> aField;
    }
    class Generic<T1, T2>
    {
        private Dictionary<T1, T2> aField;
    }
    class ClassWithArrays
    {
        private string[] strings = new string[0];
        private ClassWithFields[] withfields = new[]{new ClassWithFields()};
    }
    class ClassWithSelfUsingArray
    {
        private ClassWithSelfUsingArray[] aField = new ClassWithSelfUsingArray[0];
    }

    class ClassWithAutoProperty
    {
        private string AField { get; set; }
    }
    class NestedGenerics<T>
    {
        public class InnerGeneric<U,K>{}
    }


    class Base
    {
        private string aField;

        public string AField
        {
            get { return aField; }
            set { aField = value; }
        }
    }

    class SubClass : Base
    {
        private string subClassField;

        public string SubClassField
        {
            get { return subClassField; }
            set { subClassField = value; }
        }
    }

    class SystemTypeArrays
    {
        private DayOfWeek[] aField = new[]
                                       {
                                           DayOfWeek.Monday,
                                           DayOfWeek.Sunday,
                                       };
    }

    namespace ConflictingNamespaces
    {
        class AClass { }

        namespace SubNamespace
        {
            class AClass
            {
                private int field;
            }
        
        }
// ReSharper restore ConvertToAutoProperty
#pragma warning restore 169
}

}