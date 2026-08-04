using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuicSortBenchmark.Comparer
{
    public class NameComparer : IComparer<Person>
    {
        public int Compare(Person? x, Person? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            int val = string.Compare(x.FirstName, y.FirstName);
            if (val == 0)
            {
                val = string.Compare(x.SecondName, y.SecondName);
            }
            else
            {
                return val;
            }
            if (val == 0)
            {
                val = string.Compare(x.personalCode, y.personalCode);
            }
            else
            {
                return val;
            }
            return val;

        }
    }
}
