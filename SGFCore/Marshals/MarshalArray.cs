////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
* 描述：
* 作者：slicol
*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SGF.Marshals
{
    public class MarshalArray<T> : MarshalArrayBase, IEnumerable<T>, IEnumerable where T : struct
    {
        public MarshalArray(int length) : base(length, typeof(T))
        {
        }

        public T this[int index]
        {
            get { return GetValue<T>(index); }
            set { SetValue(index, value); }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        struct Enumerator : IEnumerator<T>
        {
            private MarshalArray<T> list;
            private int index;
            private T current;

            internal Enumerator(MarshalArray<T> list)
            {
                this.list = list;
                this.index = 0;
                this.current = default(T);
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                MarshalArray<T> list = this.list;

                if ((uint)this.index >= (uint)list.Length)
                {
                    this.index = this.list.Length + 1;
                    this.current = default(T);
                    return false;
                }

                this.current = list[this.index];
                ++this.index;
                return true;
            }

            public T Current { get { return this.current; } }

            object IEnumerator.Current
            {
                get
                {
                    if (this.index == 0 || this.index == this.list.Length + 1)
                    {
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    }
                    return (object)this.Current;
                }
            }

            void IEnumerator.Reset()
            {
                this.index = 0;
                this.current = default(T);
            }
        }
    }


    public abstract class MarshalArrayBase : IDisposable
    {
        public static long TotalMemory { get; private set; }
        private static readonly List<IDisposable> ms_cache = new List<IDisposable>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Cleanup()
        {
            foreach (var item in ms_cache)
            {
                item.Dispose();
            }
            ms_cache.Clear();
        }

        protected IntPtr m_header;
        protected int m_length;
        protected int m_elementSize;
        protected bool m_disposed = false;
        protected Type m_elementType;

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected MarshalArrayBase(int length, Type type)
        {
            m_length = length;
            m_elementType = type;
            m_elementSize = Marshal.SizeOf(m_elementType);

            int memSize = m_length * m_elementSize;
            m_header = Marshal.AllocHGlobal(memSize);
            TotalMemory += memSize;
            ms_cache.Add(this);
        }

        ~MarshalArrayBase()
        {
            Dispose();
        }

        protected void Dispose(bool disposing)
        {
            if (m_disposed)
            {
                return;
            }

            if (m_header != IntPtr.Zero)
            {
                TotalMemory -= m_length * m_elementSize;
                Marshal.FreeHGlobal(m_header);
                m_length = 0;
                m_header = IntPtr.Zero;
            }

            m_disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int Length { get { return m_length; } }
        public int ElementSize { get { return m_elementSize; } }
        public Type ElementType { get { return m_elementType; } }
        public int ByteLength { get { return m_length * m_elementSize; } }

        public unsafe void* Pointer { get { return (void*)m_header; } }
        public IntPtr Ptr { get { return m_header; } }
        public unsafe void* GetElementPointer(int index)
        {
            return (void*)((byte*)m_header + index * m_elementSize);
        }
        public unsafe IntPtr GetElementPtr(int index)
        {
            return (IntPtr)((byte*)m_header + index * m_elementSize);
        }

        public unsafe T GetValue<T>(int index)
        {
            if (index < 0 || index >= m_length)
            {
                throw new IndexOutOfRangeException("index is out of range");
            }

            var ptrSrc = ((byte*)m_header + index * m_elementSize);
            return (T)Marshal.PtrToStructure((IntPtr) ptrSrc, m_elementType);
        }
        public unsafe void SetValue<T>(int index, T value)
        {
            if (index < 0 || index >= m_length)
            {
                throw new IndexOutOfRangeException("index is out of range");
            }

            var ptrSrc = ((byte*)m_header + index * m_elementSize);
            Marshal.StructureToPtr(value, (IntPtr)ptrSrc, true);
        }

        public static int IndexOf<T>(MarshalArrayBase source, T item, int beginIndex, int count)
        {
            if (beginIndex < 0 || count < 0)
            {
                throw new ArgumentException("beginIndex < 0 or count < 0");
            }

            if (source.Length - beginIndex < count)
            {
                count = source.Length - beginIndex;
            }

            int end = beginIndex + count;
            int i = beginIndex;
            while (i < end)
            {
                if (source.GetValue<T>(i).Equals(item))
                {
                    return i;
                }
                ++i;
            }

            return -1;
        }

        public static int LastIndexOf<T>(MarshalArrayBase source, T item, int beginIndex, int count)
        {
            if (beginIndex < 0 || count < 0)
            {
                throw new ArgumentException("beginIndex < 0 or count < 0");
            }

            if (beginIndex >= source.Length)
            {
                beginIndex = source.Length - 1;
            }

            if (beginIndex + 1 < count)
            {
                count = beginIndex + 1;
            }

            int end = beginIndex - count;
            int i = beginIndex;
            while (i > end)
            {
                if (source.GetValue<T>(i).Equals(item))
                {
                    return i;
                }
                --i;
            }

            return -1;
        }

        public static int Copy<T>(MarshalArrayBase source, int sourceOffset, T[] destination, int destOffset, int count)
        {
            if (sourceOffset < 0 || destOffset < 0 || count < 0)
            {
                throw new ArgumentException("sourceOffset < 0 or destOffset < 0 or count < 0");
            }

            if (source.Length - sourceOffset < count)
            {
                count = source.Length - sourceOffset;
            }

            if (destination.Length - destOffset < count)
            {
                count = destination.Length - destOffset;
            }

            int srcIndex = sourceOffset;
            int dstIndex = destOffset;
            int srcEnd = sourceOffset + count;
            while (srcIndex < srcEnd)
            {
                destination[dstIndex] = source.GetValue<T>(srcIndex);
                ++dstIndex;
                ++srcIndex;
            }
            return count;
        }

        public static int Copy<T>(T[] source, int sourceOffset, MarshalArrayBase destination, int destOffset, int count)
        {
            if (sourceOffset < 0 || destOffset < 0 || count < 0)
            {
                throw new ArgumentException("sourceOffset < 0 or destOffset < 0 or count < 0");
            }

            if (source.Length - sourceOffset < count)
            {
                count = source.Length - sourceOffset;
            }

            if (destination.Length - destOffset < count)
            {
                count = destination.Length - destOffset;
            }

            int srcIndex = sourceOffset;
            int dstIndex = destOffset;
            int srcEnd = sourceOffset + count;
            while (srcIndex < srcEnd)
            {
                destination.SetValue<T>(dstIndex, source[srcIndex]);
                ++dstIndex;
                ++srcIndex;
            }
            return count;
        }

        public unsafe static int Copy(MarshalArrayBase source, int sourceOffset, MarshalArrayBase destination, int destOffset, int count)
        {
            if (sourceOffset < 0 || destOffset < 0 || count < 0)
            {
                throw new ArgumentException("sourceOffset < 0 or destOffset < 0 or count < 0");
            }

            if (source.Length - sourceOffset < count)
            {
                count = source.Length - sourceOffset;
            }

            if (destination.Length - destOffset < count)
            {
                count = destination.Length - destOffset;
            }


            return CopyBytes(source.Ptr, source.m_elementSize * sourceOffset,
                destination.Ptr, destination.m_elementSize * destOffset,
                source.m_elementSize * count);


        }

        public unsafe static int CopyBytes(IntPtr source, int sourceOffset, IntPtr destination, int destOffset, int count)
        {
            if (source == IntPtr.Zero)
            {
                throw new ArgumentException("source is null!");
            }
            if (destination == IntPtr.Zero)
            {
                throw new ArgumentException("destination is null");
            }
            if (sourceOffset < 0 || destOffset < 0 || count < 0)
            {
                throw new ArgumentException("sourceOffset < 0 or destOffset < 0 or count < 0");
            }

            if (count == 0)
            {
                return 0;
            }

            byte* srcPtr = (byte*)source;
            srcPtr += sourceOffset;

            byte* dstPtr = (byte*)destination;
            dstPtr += destOffset;

            if (srcPtr == dstPtr)
            {
                return count;
            }

            int byteSize = count;

            if (source != destination || srcPtr > dstPtr)
            {
                byte* srcEnd = srcPtr + byteSize;

                while (srcPtr < srcEnd)
                {
                    *dstPtr = *srcPtr;
                    ++srcPtr;
                    ++dstPtr;
                }
            }
            else
            {
                byte* srcEnd = srcPtr - 1;
                srcPtr = srcPtr + byteSize - 1;
                dstPtr = dstPtr + byteSize - 1;

                while (srcPtr > srcEnd)
                {
                    *dstPtr = *srcPtr;
                    --srcPtr;
                    --dstPtr;
                }
            }

            return count;

        }

    }



}