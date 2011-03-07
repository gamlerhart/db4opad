namespace Gamlor.Db4oPad.Tests.TestTypes
{
    public class Person
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
    }

}