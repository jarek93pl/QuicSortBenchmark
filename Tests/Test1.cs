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
        [TestMethod]
        public void SortTestName()
        {
            var tab = Generator.GenerateRandomPersons(2_000_000);
            var tab2 = (Person[])tab.Clone();

            Stopwatch stopwatch = Stopwatch.StartNew();
            Array.Sort(tab2, new NameComparer());
            Console.WriteLine($"Time taken to sort {tab.Length} Person objects using Array.Sort: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch = Stopwatch.StartNew();
            SortParallelWithProcesorParameter<Person> sortParallel = new SortParallelWithProcesorParameter<Person>(20_000_000, 800, new NameComparer());
            sortParallel.Sort(tab);
            Console.WriteLine($"Time taken to sort {tab.Length} Person objects: {stopwatch.ElapsedMilliseconds} ms");
            for (int i = 0; i < tab.Length; i++)
            {
                Assert.AreEqual(tab[i].CompareTo(tab2[i]), 0);
            }
        }
        [TestMethod]
        public void SortTestDistance()
        {
            var tab = Generator.GenerateRandomPersons(2_000_000);
            var tab2 = (Person[])tab.Clone();

            Stopwatch stopwatch = Stopwatch.StartNew();
            Array.Sort(tab2, new NameComparer());
            Console.WriteLine($"Time taken to sort {tab.Length} Person objects using Array.Sort: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch = Stopwatch.StartNew();
            SortParallelWithProcesorParameter<Person> sortParallel = new SortParallelWithProcesorParameter<Person>(20_000_000, 200, new NameComparer());
            sortParallel.Sort(tab);
            Console.WriteLine($"Time taken to sort {tab.Length} Person objects: {stopwatch.ElapsedMilliseconds} ms");
            for (int i = 0; i < tab.Length; i++)
            {
                Assert.AreEqual(tab[i].CompareTo(tab2[i]), 0);
            }
        }
        [TestMethod]
        public void TestPaging()
        {
            const int size = 10_000_000;
            var tab = Generator.GenerateRandomPersons(size);
            var tab2 = (Person[])tab.Clone();

            Stopwatch stopwatch = Stopwatch.StartNew();
            Array.Sort(tab, new NameComparer());
            Console.WriteLine($"Time taken to sort {size} Person objects using Array.Sort: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch = Stopwatch.StartNew();
            var spanPaging = SortForPage<Person>.Sort(tab2, new NameComparer(), 1000, 10000);
            var spanNet = tab.AsSpan(1000, 10000);
            Console.WriteLine($"Time taken to sort {size} Person objects: {stopwatch.ElapsedMilliseconds} ms");
            var nmComparer = new NameComparer();
            for (int i = 0; i < spanPaging.Length; i++)
            {
                Assert.AreEqual(nmComparer.Compare(spanPaging[i], spanNet[i]), 0);
            }
        }
        [TestMethod]
        public void TestPagingsmal()
        {
            var tab = Generator.GenerateRandomPersons(1000);
            var tab2 = (Person[])tab.Clone();

            Stopwatch stopwatch = Stopwatch.StartNew();
            Array.Sort(tab, new NameComparer());
            Console.WriteLine($"Time taken to sort 20,000,000 Person objects using Array.Sort: {stopwatch.ElapsedMilliseconds} ms");
            stopwatch = Stopwatch.StartNew();
            var spanPaging = SortForPage<Person>.Sort(tab2, new NameComparer(), 10, 100);
            var spanNet = tab.AsSpan(10, 100);
            Console.WriteLine($"Time taken to sort 20,000,000 Person objects: {stopwatch.ElapsedMilliseconds} ms");
            var nmComparer = new NameComparer();
            for (int i = 0; i < spanPaging.Length; i++)
            {
                Assert.AreEqual(nmComparer.Compare(spanPaging[i], spanNet[i]), 0);
            }
        }
        [TestMethod]
        public void TestMethodNamen100()
        {
            int count = 10;
            int ArraySize = 1_000_000;
            var source = Generator.GenerateRandomPersons(ArraySize);
            var tab = (Person[])source.Clone();
            var tab2 = (Person[])source.Clone();
            long timeCreate = 0;
            long timeSort = 0;
            long pastCreate = 0;
            long pastSort = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                tab2 = (Person[])source.Clone();
                pastCreate = stopwatch.ElapsedMilliseconds;
                timeCreate += pastCreate - pastSort;
                Console.WriteLine($"time1 {stopwatch.ElapsedMilliseconds}");
                Array.Sort(tab2, new NameComparer());
                Console.WriteLine($"time2 {stopwatch.ElapsedMilliseconds}");
                pastSort = stopwatch.ElapsedMilliseconds;
                timeSort += pastSort - pastCreate;

            }
            Console.WriteLine($"time sort Array.Sort: {timeSort} ms");
            Console.WriteLine($"time create Array.Sort: {timeCreate} ms");

        }
    }
}
