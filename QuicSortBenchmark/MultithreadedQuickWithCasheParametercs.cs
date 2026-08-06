
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
    List<Task> allWidleTask = new List<Task>();
    long taskDone = 0;
    long taskStarted = 0;
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
        DepthLimitedQuickSort1(arr, left, right);
        while (Interlocked.Read(ref taskDone) < numberOfProcessors)
        {
            Thread.Sleep(10);
        }
        blockingColection.CompleteAdding();
        Task.WaitAll(tasks);


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
    bool hasBeenStarted = false;
    void FirstRunTask()
    {
        if (hasBeenStarted)
        {
            return;
        }
        hasBeenStarted = true;
        foreach (var task in tasks)
        {
            task.Start();
        }
    }
    private void InvokeBlockingColection(int left, int right)
    {
        blockingColection.Add(new startAndEnd { start = left, end = right });
    }

    internal void DepthLimitedQuickSort1(T[] keys, int left, int right)
    {
        int sizeToCompute = right - left;
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


            if (j - left <= right - i)
            {
                if (CheckCreateNewTaskL1())
                {
                    if (left < j)
                    {
                        RunTaskL1(keys, left, j);
                    }
                    RunTaskL1(keys, i, right);
                }
                else
                {
                    DepthLimitedQuickSort(keys, i, right, 16);
                    DepthLimitedQuickSort(keys, left, j, 16);
                }
                return;
                left = i;
            }
            else
            {
                if (CheckCreateNewTaskL1())
                {
                    if (i < right)
                    {
                        RunTaskL1(keys, i, right);
                    }
                    RunTaskL1(keys, left, j);
                }
                else
                {
                    DepthLimitedQuickSort(keys, i, right, 16);
                    DepthLimitedQuickSort(keys, left, j, 16);
                }
                return;
                right = j;
            }
        } while (left < right);

    }

    bool CheckCreateNewTaskL1()
    {
        return Interlocked.Read(ref taskStarted) < numberOfProcessors;
    }
    private void RunTaskL1(T[] keys, int left, int right)
    {
        if (CheckCreateNewTaskL1())
        {
            Interlocked.Increment(ref taskStarted);
            Task.Run(() =>
            {
                DepthLimitedQuickSort1(keys, left, right);
                Interlocked.Increment(ref taskDone);
            });

            FirstRunTask();
        }
        else
        {
            DepthLimitedQuickSort(keys, left, right, 16);
        }
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