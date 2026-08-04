using QuicSortBenchmark;

namespace Tests
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var tab = Generator.GenerateRandomPersons(2_000_000);
            var tab2 = (Person[])tab.Clone();

            Array.Sort(tab2);
            SortParallelWithProcesorParameter<Person> sortParallel = new SortParallelWithProcesorParameter<Person>(20_000_000, 120);
            sortParallel.Sort(tab);

            for (int i = 0; i < tab.Length; i++)
            {
                Assert.AreEqual(tab[i].CompareTo(tab2[i]), 0);
            }
        }
    }
}
