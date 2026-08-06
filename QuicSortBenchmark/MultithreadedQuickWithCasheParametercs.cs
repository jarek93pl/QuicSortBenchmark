
//Csharpusing System;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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
        numberOfProcessors = Environment.ProcessorCount / 2;
        this.comparer = comparer;
        PARALLEL_THRESHOLD = sizeCashe / (sizeObject * numberOfProcessors);
        tasks = new Task[numberOfProcessors - 2];
    }
    int numberOfProcessors;
    private int PARALLEL_THRESHOLD;
    private const int minSizeForParral = 20;
    private const int MainPARALLEL_THRESHOLD = 50_000;
    BlockingCollection<startAndEnd> blockingColection = new BlockingCollection<startAndEnd>();
    Task[] tasks;
    Task second;
#if DEBUG

    bool notUsed = true;
#endif
    [Conditional("DEBUG")]
    public void CheckUsingSecondTime()
    {
#if DEBUG        
        //Contract.Assert(notUsed);
        if (!notUsed)
        {
            throw new ObjectDisposedException("this object can be used only one time");
        }
        notUsed = false;
#endif
    }
    public void Sort(T[] arr)
    {
        CheckUsingSecondTime();
        int left = 0, right = arr.Length - 1;
        if (arr.Length < MainPARALLEL_THRESHOLD)
        {
            Array.Sort(arr);
            return;
        }
        CreateTask(arr);
        PARALLEL_THRESHOLD = Math.Min(PARALLEL_THRESHOLD, arr.Length / numberOfProcessors);
        DepthLimitedQuickSort1(arr, left, right, 16);
        second?.Wait();
        blockingColection.CompleteAdding();
        Task.WaitAll(tasks);

        foreach (var range in blockingColection.GetConsumingEnumerable())
        {
            Span<T> span = arr.AsSpan(range.start, range.end - range.start + 1);
            span.Sort();
        }
    }

    private void CreateTask(T[] arr)
    {
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = new Task((() =>
            {
                while (!blockingColection.IsCompleted)
                {
                    try
                    {
                        var range = blockingColection.Take();


                        DepthLimitedQuickSortClean(arr, range.start, range.end, 32);
                    }
                    catch (Exception)
                    {

                    }

                }
            }));
        }
    }

    void RunTask()
    {
        foreach (var task in tasks)
        {
            task.Start();
        }
    }
    private void InvokeBlockingColection(int left, int right)
    {
        blockingColection.Add(new startAndEnd { start = left, end = right });
    }

    internal void DepthLimitedQuickSort1(T[] keys, int left, int right, int depthLimit, bool useBlockingColection = true)
    {
        int sizeToCompute = right - left;
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
                if (left < j)
                {
                    second = Task.Run(() => DepthLimitedQuickSort(keys, left, j, depthLimit, useBlockingColection));
                    RunTask();
                    DepthLimitedQuickSort(keys, i, right, depthLimit, useBlockingColection);
                    return;
                }
                left = i;
            }
            else
            {
                if (i < right)
                {
                    second = Task.Run(() => DepthLimitedQuickSort(keys, i, right, depthLimit, useBlockingColection));
                    RunTask();
                    DepthLimitedQuickSort(keys, left, j, depthLimit, useBlockingColection);
                    return;
                }
                right = j;
            }
        } while (left < right);
    }
    internal void DepthLimitedQuickSort(T[] keys, int left, int right, int depthLimit, bool useBlockingColection = true)
    {
        int sizeToCompute = right - left;
        if (sizeToCompute < minSizeForParral)
        {
            if (sizeToCompute > 0)
            {
                DepthLimitedQuickSortClean(keys, left, right, depthLimit);
            }
            return;
        }
        if ((useBlockingColection && right - left < PARALLEL_THRESHOLD) || depthLimit <= 0)
        {
            InvokeBlockingColection(left, right);
            return;
        }
        do
        {

            int i = left;
            int j = right;

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

            int sizeleft = j - left;
            int sizeright = right - i;//j - left <= right - i
            if (sizeleft <= sizeright)
            {
                if (left < j) DepthLimitedQuickSort(keys, left, j, depthLimit - 1, useBlockingColection);
                left = i;
                if (sizeright < PARALLEL_THRESHOLD)
                {
                    DepthLimitedQuickSort(keys, i, right, depthLimit - 1, useBlockingColection);
                    return;
                }
            }
            else
            {
                if (i < right) DepthLimitedQuickSort(keys, i, right, depthLimit - 1, useBlockingColection);
                right = j;

                if (sizeleft < PARALLEL_THRESHOLD)
                {
                    DepthLimitedQuickSort(keys, left, j, depthLimit - 1, useBlockingColection);
                    return;
                }

            }
        } while (left < right);
    }
    internal void DepthLimitedQuickSortClean(T[] keys, int left, int right, int depthLimit)
    {
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
                if (left < j) DepthLimitedQuickSortClean(keys, left, j, depthLimit);
                left = i;
            }
            else
            {
                if (i < right) DepthLimitedQuickSortClean(keys, i, right, depthLimit);
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

}