// See https://aka.ms/new-console-template for more information
using QuicSortBenchmark;
using System.Diagnostics;
using System.Drawing;

Console.WriteLine("Hello, World!");

int[] tableSizeArray = new int[] { 1000, 10000, 100000, 1_000_000, 10_000_000 };
foreach (var item in tableSizeArray)
{
    BenchmarTable<Person>(X => { Array.Sort(X); }, item, 5, () => Generator.GetPerson(), "net implemetation");
    //BenchmarTable<Person>(X => { SortParallel<Person>.QuickSortParallel(X, 0, X.Length - 1); }, item, 5, () => Generator.GetPerson(), "mutithreting");
}
Console.WriteLine("Finish");
/*

BenchmarTable<Person>((tab) =>
{
    SortParallel<Person>.QuickSortParallel(tab, 0, tab.Length - 1); foreach (var item in tab)
    {
        Console.WriteLine(item);
    }
}, 3000, 1, () => Generator.GetPerson());
*/

void BenchmarTable<T>(Action<T[]> action, int sizeArray, int numberTry, Func<T> generator, string name) where T : IComparable<T>
{
    T[] table = new T[sizeArray];
    for (int i = 0; i < sizeArray; i++)
    {
        table[i] = generator();
    }
    long sumTime = 0;
    List<long> times = new List<long>(numberTry);
    for (int i = 0; i < numberTry; i++)
    {
        var clone = (T[])table.Clone();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        action(clone);
        long result = stopwatch.ElapsedMilliseconds;
        times.Add(result);
        sumTime += result;
    }
    Console.WriteLine($"Average time {name} for {sizeArray} elements over {numberTry} tries: {sumTime / numberTry} ms");
    Console.WriteLine("name;sizeArray;time");
    foreach (var time in times)
    {
        Console.WriteLine($"{name}{sizeArray};{time}");
    }
}