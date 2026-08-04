using QuicSortBenchmark;
using QuicSortBenchmark.Comparer;
using System.Diagnostics;

namespace Tests
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var tab = Generator.GenerateRandomPersons(20_000_000);
            var tab2 = (Person[])tab.Clone();

            Stopwatch stopwatch = Stopwatch.StartNew();
            Array.Sort(tab2, new AgePersonComparer());
            Console.WriteLine($"Time taken to sort 20,000,000 Person objects using Array.Sort: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch = Stopwatch.StartNew();
            SortParallelWithProcesorParameter<Person> sortParallel = new SortParallelWithProcesorParameter<Person>(20_000_000, 400, new AgePersonComparer());
            sortParallel.Sort(tab);
            Console.WriteLine($"Time taken to sort 20,000,000 Person objects: {stopwatch.ElapsedMilliseconds} ms");
            for (int i = 0; i < tab.Length; i++)
            {
                Assert.AreEqual(tab[i].CompareTo(tab2[i]), 0);
            }
        }
    }
}
