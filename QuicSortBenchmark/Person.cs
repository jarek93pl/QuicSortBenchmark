using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuicSortBenchmark
{
    public class Person : IComparable<Person>
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string personalCode { get; set; }
        public int Age { get; set; }
        public DateTime DataBright;
        public short region;
        public addres homeAddres;
        public Person(string name, int age)
        {
            FirstName = name;
            Age = age;
        }
        public int CompareTo(Person other)
        {
            if (other == null) return 1;
            // Sort by Age in descending order
            return other.Age.CompareTo(this.Age);
        }
        public override string ToString()
        {
            return $"{Age} {FirstName} {SecondName} ";
        }
    }
    public class addres
    {
        public string street;
        public string city;
        private string v1;
        private string v2;
        private string v3;

        public addres(string v1, string v2, string v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }
}
