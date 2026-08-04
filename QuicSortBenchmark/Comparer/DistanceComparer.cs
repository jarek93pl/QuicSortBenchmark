using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QuicSortBenchmark.Comparer
{
    public class DistanceComparer : IComparer<Person>
    {
        public Vector2 pointCenter;
        public int Compare(Person? x, Person? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return Vector2.Distance(pointCenter, x.position).CompareTo(Vector2.Distance(pointCenter, y.position));


        }
    }
}
