using System;
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

        public List<string> AField
        {
            get { return aField; }
            set { aField = value; }
        }
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

    class ClassWithAutoProperty
    {
        private string AField { get; set; }
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
}