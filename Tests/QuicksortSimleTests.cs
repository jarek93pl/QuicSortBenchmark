using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass()]
    public class QuicksortSimleTests
    {
        [TestMethod()]
        public void SortTestOnlyInt1000()
        {
            const int size = 1000;
            int[] tab = new int[size];
            Random r = new Random();
            for (int i = 0; i < size; i++)
            {
                tab[i] = r.Next();
            }

            int[] tabclone = (int[])tab.Clone();

            Array.Sort(tabclone);
            QuicksortSimle.Sort(tab);
            for (int i = 0; i < tab.Length; i++)
            {
                Assert.AreEqual(tab[i], tabclone[i]);
            }
            QuicksortSimle.Sort(tab);
        }
        [TestMethod()]
        public void SortTestOnlyInt100_000_000()
        {
            const int size = 100_000_000;
            int[] tab = new int[size];
            Random r = new Random();
            for (int i = 0; i < size; i++)
            {
                tab[i] = r.Next(10_00);
            }

            int[] tabclone = (int[])tab.Clone();

            Array.Sort(tabclone);
            QuicksortSimle.Sort(tab);
            for (int i = 0; i < tab.Length; i++)
            {
                Assert.AreEqual(tab[i], tabclone[i]);
            }
            QuicksortSimle.Sort(tab);
        }
        [TestMethod()]
        public void SortTestOnlyInt100_000_000RaoundNormalTime()
        {
            const int size = 100_000_000;
            int[] tab = new int[size];
            Random r = new Random();
            for (int i = 0; i < size; i++)
            {
                tab[i] = r.Next(10_00);
            }

            Stopwatch sp = Stopwatch.StartNew();
            Array.Sort(tab);
            Console.WriteLine(sp.ElapsedMilliseconds);
        }
    }
}