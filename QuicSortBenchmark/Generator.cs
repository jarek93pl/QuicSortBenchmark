using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuicSortBenchmark.StatisticData;

namespace QuicSortBenchmark
{
    public static class Generator
    {
        static SortedDictionary<RangeWithString, string> PolishFirstNames;
        static SortedDictionary<RangeWithString, string> PolishSecondNames;
        static int numberMax;
        static Generator()
        {

            PolishFirstNames = StatisticData.LoadPolishFirstName(out int numberOfFirstName);

            PolishSecondNames = StatisticData.LoadPolishSecondName(out int numberofSecondName);
            numberMax = Math.Max(numberofSecondName, numberOfFirstName) + 10000;
        }
        static Random rand = new Random();
        public static Person GetPerson()
        {
            Person person = new Person(randomString(), rand.Next(0, 100));
            person.DataBright = DateTime.Now.AddDays(-rand.Next(0, 365 * 100));
            person.region = (short)rand.Next(1, 100);
            person.homeAddres = new addres(randomString(), randomString(), randomString());
            person.personalCode = loadPolishFirstName();
            person.SecondName = loadPolishSecondName();
            person.position = new System.Numerics.Vector2(rand.NextSingle() * 100, rand.NextSingle() * 100);
            return person;


        }
        public static string loadPolishFirstName()
        {
            if (PolishFirstNames.TryGetValue(new StatisticData.RangeWithString { center = rand.Next(numberMax) }, out var value))
            {
                return value;
            }
            else
            {
                return randomString();
            }
        }
        public static string loadPolishSecondName()
        {
            if (PolishSecondNames.TryGetValue(new StatisticData.RangeWithString { center = rand.Next(numberMax) }, out var value))
            {
                return value;
            }
            else
            {
                return randomString();
            }
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
