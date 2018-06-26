using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SGF.G3Lite;
using SGF.Marshals;

namespace SGFCoreTest
{
    [TestClass]
    public class TestMarshalArray
    {
        [TestCleanup]
        public void Cleanup()
        {
            MarshalArrayBase.Cleanup();
        }


        [TestMethod]
        public void TestSetterGetterInt()
        {
            int count = 100;
            MarshalArray<int> list = new MarshalArray<int>(count);
            Assert.AreEqual(list.Length, count);

            for (int i = 0; i < count; i++)
            {
                list[i] = i;
            }

            Assert.AreEqual(list.Length, count);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(list[i], i);
            }
        }


        [TestMethod]
        public void TestSetterGetterFloat()
        {
            int count = 100;
            MarshalArray<float> list = new MarshalArray<float>(count);
            Assert.AreEqual(list.Length, count);

            for (int i = 0; i < count; i++)
            {
                list[i] = i;
            }

            Assert.AreEqual(list.Length, count);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(list[i], i);
            }
        }

        [TestMethod]
        public void TestSetterGetterVector()
        {
            int count = 100;
            MarshalArray<Vector3> list = new MarshalArray<Vector3>(count);
            Assert.AreEqual(list.Length, count);

            for (int i = 0; i < count; i++)
            {
                list[i] = new Vector3(i, i, i);
            }

            Assert.AreEqual(list.Length, count);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(list[i], new Vector3(i, i, i));
            }
        }

        [TestMethod]
        public void TestIndexOf()
        {
            int count = 100;
            MarshalArray<Vector3> list = new MarshalArray<Vector3>(count);
            Assert.AreEqual(list.Length, count);

            for (int i = 0; i < count; i++)
            {
                list[i] = new Vector3(i, i, i);
            }

            int result = -1;

            result = MarshalArrayBase.IndexOf(list, new Vector3(5, 5, 5), 0, list.Length);
            Assert.AreEqual(result, 5);

            result = MarshalArrayBase.IndexOf(list, new Vector3(5, 5, 5), 5, list.Length);
            Assert.AreEqual(result, 5);

            result = MarshalArrayBase.IndexOf(list, new Vector3(5, 5, 5), 6, list.Length);
            Assert.AreEqual(result, -1);

            result = MarshalArrayBase.IndexOf(list, new Vector3(5, 5, 5), 5, 1);
            Assert.AreEqual(result, 5);

            result = MarshalArrayBase.IndexOf(list, new Vector3(5, 5, 5), 5, 0);
            Assert.AreEqual(result, -1);

            result = MarshalArrayBase.IndexOf(list, new Vector3(5, 5, 5), 0, 6);
            Assert.AreEqual(result, 5);

            result = MarshalArrayBase.IndexOf(list, new Vector3(5, 5, 5), 0, 3);
            Assert.AreEqual(result, -1);
        }


        [TestMethod]
        public void TestLastIndexOf()
        {
            int count = 100;
            MarshalArray<Vector3> list = new MarshalArray<Vector3>(count);
            Assert.AreEqual(list.Length, count);

            for (int i = 0; i < count; i++)
            {
                list[i] = new Vector3(i, i, i);
            }

            int result = -1;

            result = MarshalArrayBase.LastIndexOf(list, new Vector3(5, 5, 5), 0, list.Length);
            Assert.AreEqual(result, -1);

            result = MarshalArrayBase.LastIndexOf(list, new Vector3(5, 5, 5), 5, list.Length);
            Assert.AreEqual(result, 5);

            result = MarshalArrayBase.LastIndexOf(list, new Vector3(5, 5, 5), 6, list.Length);
            Assert.AreEqual(result, 5);

            result = MarshalArrayBase.LastIndexOf(list, new Vector3(5, 5, 5), 5, 1);
            Assert.AreEqual(result, 5);

            result = MarshalArrayBase.LastIndexOf(list, new Vector3(5, 5, 5), 5, 0);
            Assert.AreEqual(result, -1);

            result = MarshalArrayBase.LastIndexOf(list, new Vector3(5, 5, 5), 0, 6);
            Assert.AreEqual(result, -1);

            result = MarshalArrayBase.LastIndexOf(list, new Vector3(5, 5, 5), 0, 3);
            Assert.AreEqual(result, -1);
        }

        [TestMethod]
        public void TestCopyMarshal2Marshal()
        {
            int count = 100;
            MarshalArray<Vector3> list = new MarshalArray<Vector3>(count);
            Assert.AreEqual(list.Length, count);

            for (int i = 0; i < count; i++)
            {
                list[i] = new Vector3(i, i, i);
            }

            MarshalArray<Vector3> list2 = new MarshalArray<Vector3>(count);
            MarshalArrayBase.Copy(list, 0, list2, 0, count);
            Assert.AreEqual(list2.Length, count);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(list2[i], new Vector3(i, i, i));
            }


            MarshalArray<Vector3> list3 = new MarshalArray<Vector3>(10);
            MarshalArrayBase.Copy(list2, 10, list3, 5, 10);
            Assert.AreEqual(list3[5], new Vector3(10, 10, 10));
            Assert.AreEqual(list3[9], new Vector3(14, 14, 14));
        }

        [TestMethod]
        public void TestCopyManaged2Marshal()
        {
            int count = 100;
            MarshalArray<Vector3> list = new MarshalArray<Vector3>(count);
            Assert.AreEqual(list.Length, count);

            for (int i = 0; i < count; i++)
            {
                list[i] = new Vector3(i, i, i);
            }


            Vector3[] list2 = new Vector3[count];
            MarshalArrayBase.Copy(list, 0, list2, 0, count);
            Assert.AreEqual(list2.Length, count);
            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(list2[i], new Vector3(i, i, i));
            }


            MarshalArray<Vector3> list3 = new MarshalArray<Vector3>(10);
            MarshalArrayBase.Copy(list2, 10, list3, 5, 10);
            Assert.AreEqual(list3[5], new Vector3(10, 10, 10));
            Assert.AreEqual(list3[9], new Vector3(14, 14, 14));
        }

        [TestMethod]
        public void TestCopyIntPtr2IntPtr()
        {
            int count = 100;
            MarshalArray<Vector3> list = new MarshalArray<Vector3>(count);
            Assert.AreEqual(list.Length, count);

            for (int i = 0; i < count; i++)
            {
                list[i] = new Vector3(i, i, i);
            }


            MarshalArray<Vector3> list2 = new MarshalArray<Vector3>(count);
            MarshalArrayBase.CopyBytes(list.Ptr, list.ElementSize, list2.Ptr, 0, list.ByteLength);
            Assert.AreEqual(list2.Length, count);
            for (int i = 0; i < count - 1; i++)
            {
                Assert.AreEqual(list2[i], new Vector3(i + 1, i + 1, i + 1));
            }
        }


        [TestMethod]
        public void TestForEach()
        {
            int count = 100;
            MarshalArray<Vector3> list = new MarshalArray<Vector3>(count);
            Assert.AreEqual(list.Length, count);

            for (int i = 0; i < count; i++)
            {
                list[i] = new Vector3(i, i, i);
            }

            int j = 0;
            foreach (var vector3 in list)
            {
                Assert.AreEqual(vector3, new Vector3(j, j, j));
                j++;
            }

        }
    }
}
