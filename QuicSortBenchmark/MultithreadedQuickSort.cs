/* error 
//Csharpusing System;
using System.Threading.Tasks;

static class SortParallel<T> where T : IComparable<T>
{
    // Threshold to decide when to switch to sequential sort
    private const int PARALLEL_THRESHOLD = 1000;

    public static void QuickSortParallel(T[] arr, int left, int right)
    {
        if (left >= right) return;

        int pivotIndex = Partition(arr, left, right);

        // If the partition size is large enough, sort in parallel
        if ((right - left) > PARALLEL_THRESHOLD)
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
            if (arr[j].CompareTo(pivot) >= 0)
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
*/