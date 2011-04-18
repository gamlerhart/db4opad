using System;

namespace Gamlor.Db4oPad.Tests.TestTypes
{
    public class Person : IEquatable<Person>
    {
        private string firstName;
        private string sirname;
        private int age;

        public Person()
            : this("", "", 21)
        {

        }

        public Person(string firstName, string sirname, int age)
        {
            this.firstName = firstName;
            this.sirname = sirname;
            this.age = age;
        }

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string Sirname
        {
            get { return sirname; }
            set { sirname = value; }
        }

        public int Age
        {
            get { return age; }
            set { age = value; }
        }

        public bool Equals(Person other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.firstName, firstName) && Equals(other.sirname, sirname) && other.age == age;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Person)) return false;
            return Equals((Person) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (firstName != null ? firstName.GetHashCode() : 0);
                result = (result*397) ^ (sirname != null ? sirname.GetHashCode() : 0);
                result = (result*397) ^ age;
                return result;
            }
        }

        public static bool operator ==(Person left, Person right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Person left, Person right)
        {
            return !Equals(left, right);
        }
    }

}