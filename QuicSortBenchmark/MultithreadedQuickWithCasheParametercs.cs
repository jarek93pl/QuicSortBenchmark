
//Csharpusing System;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

public class SortParallelWithProcesorParameter<T> where T : IComparable<T>
{
    struct startAndEnd
    {
        public int start;
        public int end;
    }
    IComparer<T> comparer;
    public SortParallelWithProcesorParameter(int sizeCashe, int sizeObject, IComparer<T> comparer)
    {
        this.comparer = comparer;
        PARALLEL_THRESHOLD = sizeCashe / (sizeObject * numberOfProcessors);
        tasks = new Task[numberOfProcessors - 2];
    }
    int numberOfProcessors = Environment.ProcessorCount / 2;
    private readonly int PARALLEL_THRESHOLD;
    private const int MainPARALLEL_THRESHOLD = 50_000;
    BlockingCollection<startAndEnd> blockingColection = new BlockingCollection<startAndEnd>();
    Task[] tasks;
    bool started = false;
    public void Sort(T[] arr)
    {
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = (new Task(() =>
            {
                while (!blockingColection.IsCompleted)
                {
                    while (blockingColection.TryTake(out startAndEnd range))
                    {
                        DepthLimitedQuickSort(arr, range.start, range.end, 32, false);
                    }
                }
            }));
        }
        int left = 0, right = arr.Length - 1;
        if (arr.Length < MainPARALLEL_THRESHOLD)
        {
            Array.Sort(arr);
            return;
        }
        DepthLimitedQuickSort(arr, left, right, 32);
        blockingColection.CompleteAdding();
        if (!started)
        {
            return;
        }
        Task.WaitAll(tasks);
        foreach (var range in blockingColection.GetConsumingEnumerable())
        {
            Span<T> span = arr.AsSpan(range.start, range.end - range.start + 1);
            span.Sort();
        }
    }

    private void InvokeBlockingColection(int left, int right)
    {

        if (!started)
        {
            lock (blockingColection)
            {
                if (!started)
                {
                    foreach (var task in tasks)
                    {
                        task.Start();
                    }
                }
                started = true;
            }
        }
        blockingColection.Add(new startAndEnd { start = left, end = right });
    }

    internal void DepthLimitedQuickSort(T[] keys, int left, int right, int depthLimit, bool useBlockingColection = true)
    {
        if (useBlockingColection && right - left < PARALLEL_THRESHOLD)
        {
            InvokeBlockingColection(left, right);
            return;
        }
        do
        {

            int i = left;
            int j = right;

            if (depthLimit == 0)
            {
                Heapsort(keys, left, right);
                return;
            }
            // pre-sort the low, middle (pivot), and high values in place.
            // this improves performance in the face of already sorted data, or 
            // data that is made up of multiple sorted runs appended together.
            int middle = i + ((j - i) >> 1);
            SwapIfGreater(keys, comparer, i, middle);  // swap the low with the mid point
            SwapIfGreater(keys, comparer, i, j);   // swap the low with the high
            SwapIfGreater(keys, comparer, middle, j); // swap the middle with the high

            T x = keys[middle];
            do
            {
                while (comparer.Compare(keys[i], x) < 0) i++;
                while (comparer.Compare(x, keys[j]) < 0) j--;
                Contract.Assert(i >= left && j <= right, "(i>=left && j<=right)  Sort failed - Is your IComparer bogus?");
                if (i > j) break;
                if (i < j)
                {
                    T key = keys[i];
                    keys[i] = keys[j];
                    keys[j] = key;
                }
                i++;
                j--;
            } while (i <= j);


            if (j - left <= right - i)
            {
                if (left < j) DepthLimitedQuickSort(keys, left, j, depthLimit, useBlockingColection);
                left = i;
            }
            else
            {
                if (i < right) DepthLimitedQuickSort(keys, i, right, depthLimit, useBlockingColection);
                right = j;
            }
        } while (left < right);
    }

    private static void Heapsort(T[] keys, int lo, int hi)
    {
        Contract.Requires(keys != null);
        Contract.Requires(lo >= 0);
        Contract.Requires(hi > lo);
        Contract.Requires(hi < keys.Length);

        int n = hi - lo + 1;
        for (int i = n / 2; i >= 1; i = i - 1)
        {
            DownHeap(keys, i, n, lo);
        }
        for (int i = n; i > 1; i = i - 1)
        {
            Swap(keys, lo, lo + i - 1);
            DownHeap(keys, 1, i - 1, lo);
        }
    }

    private static void DownHeap(T[] keys, int i, int n, int lo)
    {
        Contract.Requires(keys != null);
        Contract.Requires(lo >= 0);
        Contract.Requires(lo < keys.Length);

        T d = keys[lo + i - 1];
        int child;
        while (i <= n / 2)
        {
            child = 2 * i;
            if (child < n && (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(keys[lo + child]) < 0))
            {
                child++;
            }
            if (keys[lo + child - 1] == null || keys[lo + child - 1].CompareTo(d) < 0)
                break;
            keys[lo + i - 1] = keys[lo + child - 1];
            i = child;
        }
        keys[lo + i - 1] = d;
    }
    private static void SwapIfGreater(T[] keys, IComparer<T> comparer, int a, int b)
    {
        if (a != b)
        {
            if (comparer.Compare(keys[a], keys[b]) > 0)
            {
                T key = keys[a];
                keys[a] = keys[b];
                keys[b] = key;
            }
        }
    }

    static int Partition(T[] arr, int left, int right, out bool isSorted)
    {
        isSorted = true;
        int difrence;
        T pivot = arr[right];
        int i = left - 1;

        ;
        for (int j = left; j < right; j++)
        {
            difrence = arr[j].CompareTo(pivot);
            if (difrence >= 0)
            {
                i++;
                Swap(arr, i, j);
            }
            else if (difrence != 0)
            {
                isSorted = false;
            }
        }
        Swap(arr, i + 1, right);
        return i + 1;
    }

    /// <summary>
    /// Swap two elements in the array
    /// </summary>
    static void Swap(T[] arr, int i, int j)
    {
        if (i != j)
        {
            T temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }
    }

    /// <summary>
    /// Utility method to print the array
    /// </summary>
    static void PrintArray(int[] arr)
    {
        Console.WriteLine(string.Join(", ", arr));
    }
}