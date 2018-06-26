using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SGF.Marshals;

namespace SGFCoreTest
{
    [TestClass]
    public class TestMarshalList
    {
        [TestCleanup]
        public void Cleanup()
        {
            MarshalArrayBase.Cleanup();
        }


        [TestMethod]
        public void TestAdd()
        {
            MarshalList<int> list = new MarshalList<int>();
            Assert.AreEqual(list.Capacity, 4);
            Assert.AreEqual(list.Count, 0);

            for (int i = 0; i < 10; i++)
            {
                list.Add(i);
            }
            Assert.AreEqual(list.Count, 10);
            Assert.AreEqual(list.Capacity, 16);

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(list[i], i);
            }
        }

        [TestMethod]
        public void TestAddRange()
        {
            MarshalList<int> list = new MarshalList<int>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }

            MarshalList<int> list2 = new MarshalList<int>();
            list2.AddRange(list);
            Assert.AreEqual(list2.Count, 100);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(list2[i], i);
            }

            list2.AddRange(list);
            Assert.AreEqual(list2.Count, 200);
            for (int i = 100; i < 200; i++)
            {
                Assert.AreEqual(list2[i], i - 100);
            }

            list2.AddRange(list2);
            Assert.AreEqual(list2.Count, 400);
            for (int i = 200; i < 300; i++)
            {
                Assert.AreEqual(list2[i], i - 200);
            }

            int[] list3 = new int[100];
            for (int i = 0; i < 100; i++)
            {
                list3[i] = 10000 + i;
            }
            list.AddRange(list3);
            for (int i = 100; i < 200; i++)
            {
                Assert.AreEqual(list[i], 10000 + i - 100);
            }


            List<int> list4 = new List<int>();
            list4.AddRange(list3);
            list.AddRange(list4);
            for (int i = 200; i < 300; i++)
            {
                Assert.AreEqual(list[i], 10000 + i - 200);
            }

        }

        [TestMethod]
        public void TestInsert()
        {
            MarshalList<int> list = new MarshalList<int>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }

            list.Insert(100, 10001);
            list.Insert(100, 10002);
            Assert.AreEqual(list.Count, 102);
            Assert.AreEqual(list[100], 10002);
            Assert.AreEqual(list[101], 10001);

            list.Insert(0, 10003);
            Assert.AreEqual(list[0], 10003);
            Assert.AreEqual(list[1], 0);
            Assert.AreEqual(list[2], 1);
            Assert.AreEqual(list[99], 98);
            Assert.AreEqual(list[102], 10001);
        }

        [TestMethod]
        public void TestRemove()
        {
            MarshalList<int> list = new MarshalList<int>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }

            Assert.AreEqual(list.Remove(2), true);
            Assert.AreEqual(list.Count, 99);
            Assert.AreEqual(list[2], 3);

            Assert.AreEqual(list.Remove(2), false);

            list.RemoveAt(0);
            Assert.AreEqual(list[0], 1);
            list.RemoveAt(97);
            Assert.AreEqual(list.Count, 97);

            list.RemoveRange(0, 96);
            Assert.AreEqual(list.Count, 1);
            Assert.AreEqual(list[0], 98);
        }

        [TestMethod]
        public void TestContains()
        {
            MarshalList<int> list = new MarshalList<int>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }

            Assert.AreEqual(list.Contains(5), true);
            Assert.AreEqual(list.Contains(500), false);
        }

        [TestMethod]
        public void TestCopyToManagedArray()
        {
            MarshalList<int> list = new MarshalList<int>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }

            int[] list1 = new int[100];
            list.CopyTo((Array)list1, 0);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(list1[i], i);
            }
        }


        [TestMethod]
        public void TestForEach()
        {
            MarshalList<int> list = new MarshalList<int>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }

            int j = 0;
            foreach (var i in list)
            {
                Assert.AreEqual(i, j);
                ++j;
            }
        }

        [TestMethod]
        public void TestGetRange()
        {
            MarshalList<int> list = new MarshalList<int>();
            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }

            var list1 = list.GetRange(50, 50);
            Assert.AreEqual(list1.Count, 50);
            for (int i = 0; i < 50; i++)
            {
                Assert.AreEqual(list1[i], i + 50);
            }

        }





    }
}
