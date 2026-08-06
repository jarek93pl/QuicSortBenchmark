
//Csharpusing System;
using QuicSortBenchmark;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

public class SortForPage<T>
{
    public static Span<T> Sort(T[] arr, IComparer<T> comparer, int start, int size)
    {
        DepthLimitedQuickSort(arr, 0, arr.Length - 1, 32, start - 1, start + size + 1, comparer);
        return arr.AsSpan(start, size);
    }


    static internal void DepthLimitedQuickSort(T[] keys, int left, int right, int depthLimit, int leftSidePage, int rightSizePage, IComparer<T> comparer)
    {
        if (right < leftSidePage || left > rightSizePage)
        {
            return;
        }
        do
        {

            int i = left;
            int j = right;

            if (depthLimit == 0)
            {
                Span<T> span = keys.AsSpan(left, right - left);
                span.Sort(comparer);
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
                if (j > leftSidePage)
                {
                    if (left < j)
                    {
                        DepthLimitedQuickSort(keys, left, j, depthLimit, leftSidePage, rightSizePage, comparer);
                    }
                }
                left = i;
            }
            else
            {
                if (rightSizePage > i)
                {
                    if (i < right) DepthLimitedQuickSort(keys, i, right, depthLimit, leftSidePage, rightSizePage, comparer);
                }
                right = j;
            }
        } while (left < right);
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

    /// <summary>
    /// Utility method to print the array
    /// </summary>
    static void PrintArray(int[] arr)
    {
        Console.WriteLine(string.Join(", ", arr));
    }
}