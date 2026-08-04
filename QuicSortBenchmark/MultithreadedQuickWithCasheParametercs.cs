
//Csharpusing System;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class SortParallelWithProcesorParameter<T> where T : IComparable<T>
{
    struct startAndEnd
    {
        public int start;
        public int end;
    }
    public SortParallelWithProcesorParameter(int sizeCashe, int sizeObject)
    {
        PARALLEL_THRESHOLD = sizeCashe / (sizeObject * numberOfProcessors);
        tasks = new Task[numberOfProcessors - 2];
    }
    int numberOfProcessors = Environment.ProcessorCount;
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
                        Span<T> span = arr.AsSpan(range.start, range.end - range.start + 1);
                        span.Sort();
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
        int pivotIndex = Partition(arr, left, right);


        Parallel.Invoke(
            () => QuickSortParallel(arr, left, pivotIndex - 1),
            () => QuickSortParallel(arr, pivotIndex + 1, right)
        );
        Task.WaitAll(tasks);
        foreach (var range in blockingColection.GetConsumingEnumerable())
        {
            Span<T> span = arr.AsSpan(range.start, range.end - range.start + 1);
            span.Sort();
        }
    }
    public void QuickSortParallel(T[] arr, int left, int right)
    {

        if (left >= right) return;

        int size = right - left + 1;
        if (size > PARALLEL_THRESHOLD)
        {
            int pivotIndex = Partition(arr, left, right);
            QuickSortParallel(arr, left, pivotIndex - 1);
            QuickSortParallel(arr, pivotIndex + 1, right);

        }
        else if (size < 200)
        {
            Span<T> span = arr.AsSpan(left, size);
            span.Sort();
        }
        else
        {
            if (!started)
            {
                lock (blockingColection)
                {
                    started = true;
                    if (!started)
                    {
                        foreach (var task in tasks)
                        {
                            task.Start();
                        }
                    }
                }
            }
            blockingColection.Add(new startAndEnd { start = left, end = right });
        }
    }
    public void QuickSortBigArray(T[] arr, int left, int right)
    {

        if (left >= right) return;

        int pivotIndex = Partition(arr, left, right);

        // If the partition size is large enough, sort in parallel
        if ((right - left) > MainPARALLEL_THRESHOLD)
        {
            Parallel.Invoke(
                () => QuickSortParallel(arr, left, pivotIndex - 1),
                () => QuickSortParallel(arr, pivotIndex + 1, right)
            );
        }
        else
        {
            // Sort sequentially for small partitions
            QuickSortParallel(arr, left, pivotIndex - 1);
            QuickSortParallel(arr, pivotIndex + 1, right);
        }
    }

    /// <summary>
    /// Partition method for QuickSort
    /// </summary>
    static int Partition(T[] arr, int left, int right)
    {
        T pivot = arr[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (arr[j].CompareTo(pivot) > 0)
            {
                i++;
                Swap(arr, i, j);
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