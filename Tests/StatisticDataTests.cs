using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuicSortBenchmark;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuicSortBenchmark.Tests
{
    [TestClass()]
    public class StatisticDataTests
    {
        [TestMethod()]
        public void LoadPolishFirstNameTest()
        {
            var dictionary = StatisticData.LoadPolishFirstName(out int number);
            {
                if (dictionary.TryGetValue(new StatisticData.RangeWithString { center = 500 }, out var value))
                {
                    Assert.AreEqual(value, "Anna");
                }
            }
            {
                if (dictionary.TryGetValue(new StatisticData.RangeWithString { center = 2_000_000 }, out var value))
                {
                    Assert.AreEqual(value, "Krzysztof");
                }
            }

            {
                if (dictionary.TryGetValue(new StatisticData.RangeWithString { center = 36_000_000 }, out var value))
                {
                    Assert.AreEqual(value, "Henryka");
                }
            }
        }
        [TestMethod()]
        public void LoadPolishSecondNameTest()
        {
            var dictionary = StatisticData.LoadPolishSecondName(out int number);
            {
                if (dictionary.TryGetValue(new StatisticData.RangeWithString { center = 500 }, out var value))
                {
                    Assert.AreEqual(value, "NOWAK");
                }
            }
            {
                if (dictionary.TryGetValue(new StatisticData.RangeWithString { center = 2_000_000 }, out var value))
                {
                    Assert.AreEqual(value, "MRÓZ");
                }
            }

            {
                if (dictionary.TryGetValue(new StatisticData.RangeWithString { center = 20_000_000 }, out var value))
                {
                    Assert.AreEqual(value, "SALADKOU");
                }
            }
        }
    }
}