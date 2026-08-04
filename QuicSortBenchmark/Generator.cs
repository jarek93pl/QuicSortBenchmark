using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuicSortBenchmark
{
    public static class Generator
    {
        static Random rand = new Random();
        public static Person GetPerson()
        {
            Person person = new Person(randomString(), rand.Next(0, 100));
            person.DataBright = DateTime.Now.AddDays(-rand.Next(0, 365 * 100));
            person.region = (short)rand.Next(1, 100);
            person.homeAddres = new addres(randomString(), randomString(), randomString());
            person.personalCode = randomString();
            person.SecondName = randomString();
            person.position = new System.Numerics.Vector2(rand.NextSingle() * 100, rand.NextSingle() * 100);
            return person;


        }
        public static string randomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, rand.Next(4, 20))
              .Select(s => s[rand.Next(s.Length)]).ToArray());
        }
        public static Person[] GenerateRandomPersons(int count)
        {
            Person[] persons = new Person[count];
            for (int i = 0; i < count; i++)
            {
                persons[i] = GetPerson();
            }
            return persons;
        }
    }
}
