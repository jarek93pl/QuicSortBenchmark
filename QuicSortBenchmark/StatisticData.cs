using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuicSortBenchmark
{
    public static class StatisticData
    {
        public struct RangeWithString : IComparable<RangeWithString>, IEquatable<RangeWithString>
        {
            public int start;
            public int end;
            public int center;
            public string name;

            public int CompareTo(RangeWithString other)
            {
                if (Equals(other))
                {
                    return 0;
                }
                return center.CompareTo(other.center);
            }

            public bool Equals(RangeWithString other)
            {
                if (other.center >= start && other.center <= end)
                {
                    return true;
                }
                if (center >= other.start && center <= other.end)
                {
                    return true;
                }
                return false;
            }
        }
        public static SortedDictionary<RangeWithString, string> LoadPolishFirstName(out int numberLast)
        {
            SortedDictionary<RangeWithString, string> ranges = new SortedDictionary<RangeWithString, string>();
            var lines = File.ReadAllLines("LoadData.txt");
            int lastNumber = 0;
            foreach (var line in lines)
            {
                var parts = line.Split('\t');
                RangeWithString rangeWithString = new RangeWithString();
                rangeWithString.start = lastNumber;
                lastNumber += Convert.ToInt32(parts[1].Replace(" ", ""));
                rangeWithString.end = lastNumber;
                lastNumber++;
                rangeWithString.center = (rangeWithString.start + rangeWithString.end) / 2;
                rangeWithString.name = parts[0];
                ranges.Add(rangeWithString, rangeWithString.name);
            }
            numberLast = lastNumber;
            return ranges;
        }

        public static SortedDictionary<RangeWithString, string> LoadPolishSecondName(out int numberLast)
        {
            SortedDictionary<RangeWithString, string> ranges = new SortedDictionary<RangeWithString, string>();
            var lines = File.ReadAllLines("SecondName.csv");
            int lastNumber = 0;
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                RangeWithString rangeWithString = new RangeWithString();
                rangeWithString.start = lastNumber;
                lastNumber += Convert.ToInt32(parts[1]);
                rangeWithString.end = lastNumber;
                lastNumber++;
                rangeWithString.center = (rangeWithString.start + rangeWithString.end) / 2;
                rangeWithString.name = parts[0];
                ranges.Add(rangeWithString, rangeWithString.name);
            }
            numberLast = lastNumber;
            return ranges;
        }
    }
}
