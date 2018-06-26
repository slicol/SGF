//参照System.Collections.Generic.List实现的

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using SGF.Utils;


namespace SGF.Marshals
{
    public unsafe class MarshalList<T> : IList<T>, IList, IDisposable where T : struct
    {
        private MarshalArray<T> m_items;
        private int m_size;
        private int m_version;
        private object m_syncRoot;

        public MarshalList()
        {
            this.m_items = new MarshalArray<T>(4);
        }

        public MarshalList(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentException("CapacityStep不能小于0！");
            }

            this.m_items = new MarshalArray<T>(capacity);
        }

        public void Dispose()
        {
            m_items.Dispose();
        }

        public int Capacity
        {
            get
            {
                return m_items.Length;
            }
            set
            {
                if (value == m_items.Length) return;

                if (value < this.m_size)
                {
                    throw new ArgumentException("新的Capacity不能小于已有数据长度！");
                }

                if (value > 0)
                {
                    var objArray = new MarshalArray<T>(value);
                    if (this.m_size > 0) MarshalArrayBase.Copy(this.m_items, 0, objArray, 0, this.m_size);
                    m_items.Dispose();

                    this.m_items = objArray;
                }
                else
                {
                    m_items.Dispose();
                    this.m_items = new MarshalArray<T>(4);
                }
            }
        }

        public int Count { get { return this.m_size; } }

        private void EnsureCapacity(int min)
        {
            if (this.m_items.Length >= min)
                return;
            int num = this.m_items.Length == 0 ? 4 : this.m_items.Length * 2;
            if (num < min)
                num = min;
            this.Capacity = num;
        }
        public void TrimExcess()
        {
            if (this.m_size >= (int)((double)this.m_items.Length * 0.9))
            {
                return;
            }

            this.Capacity = this.m_size;
        }


        #region ////IList////

        bool IList.IsFixedSize { get { return false; } }
        bool IList.IsReadOnly { get { return false; } }
        object IList.this[int index]
        {
            get
            {
                return (object)this[index];
            }
            set
            {
                VerifyValueType(value);
                this[index] = (T)value;
            }
        }
        int IList.Add(object item)
        {
            VerifyValueType(item);
            this.Add((T)item);
            return this.Count - 1;
        }
        bool IList.Contains(object item)
        {
            if (IsCompatibleObject(item))
                return this.Contains((T)item);
            return false;
        }
        int IList.IndexOf(object item)
        {
            if (IsCompatibleObject(item))
                return this.IndexOf((T)item);
            return -1;
        }
        void IList.Insert(int index, object item)
        {
            VerifyValueType(item);
            this.Insert(index, (T)item);
        }
        void IList.Remove(object item)
        {
            if (!IsCompatibleObject(item))
                return;
            this.Remove((T)item);
        }

        #endregion

        #region ////ICollection////
        bool ICollection<T>.IsReadOnly { get { return false; } }
        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot
        {
            get
            {
                if (this.m_syncRoot == null)
                    Interlocked.CompareExchange(ref this.m_syncRoot, new object(), (object)null);
                return this.m_syncRoot;
            }
        }
        #endregion

        #region ////IEnumerator////
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        #endregion

        #region ////取值////

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)this.m_size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }

                return this.m_items[index];
            }
            set
            {
                if ((uint)index >= (uint)this.m_size)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }

                this.m_items[index] = value;
                ++this.m_version;
            }
        }
        public MarshalList<T> GetRange(int index, int count)
        {
            if (index < 0 || count < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(
                    index < 0 ? ExceptionArgument.index : ExceptionArgument.count,
                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (this.m_size - index < count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }

            MarshalList<T> objList = new MarshalList<T>(count);
            MarshalArrayBase.Copy(this.m_items, index, objList.m_items, 0, count);
            objList.m_size = count;
            return objList;
        }
        public T[] ToArray()
        {
            T[] objArray = new T[this.m_size];
            MarshalArrayBase.Copy(this.m_items, 0, objArray, 0, this.m_size);
            return objArray;
        }
        public bool Contains(T item)
        {
            EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
            for (int index = 0; index < this.m_size; ++index)
            {
                if (equalityComparer.Equals(this.m_items[index], item))
                    return true;
            }
            return false;
        }


        #endregion

        #region ////Add/Insert////

        public void Add(T item)
        {
            if (this.m_size == this.m_items.Length)
            {
                this.EnsureCapacity(this.m_size + 1);
            }

            this.m_items[this.m_size++] = item;
            ++this.m_version;
        }
        public void AddRange(IEnumerable<T> collection)
        {
            this.InsertRange(this.m_size, collection);
        }


        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)this.m_size)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert);
            if (this.m_size == this.m_items.Length)
                this.EnsureCapacity(this.m_size + 1);
            if (index < this.m_size)
                MarshalArrayBase.Copy(this.m_items, index, this.m_items, index + 1, this.m_size - index);
            this.m_items[index] = item;
            ++this.m_size;
            ++this.m_version;
        }
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
            if ((uint)index > (uint)this.m_size)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            ICollection<T> objs = collection as ICollection<T>;
            if (objs != null)
            {
                int count = objs.Count;
                if (count > 0)
                {
                    this.EnsureCapacity(this.m_size + count);
                    if (index < this.m_size)
                        MarshalArrayBase.Copy(this.m_items, index, this.m_items, index + count, this.m_size - index);
                    if (this == objs)
                    {
                        MarshalArrayBase.Copy(this.m_items, 0, this.m_items, index, index);
                        MarshalArrayBase.Copy(this.m_items, index + count, this.m_items, index * 2, this.m_size - index);
                    }
                    else
                    {
                        T[] array = new T[count];
                        objs.CopyTo(array, 0);
                        MarshalArrayBase.Copy<T>(array, 0, m_items, index, count);
                    }
                    this.m_size += count;
                }
            }
            else
            {
                foreach (T obj in collection)
                {
                    this.Insert(index++, obj);
                }
            }
            ++this.m_version;
        }

        #endregion

        #region ////Remove/Clear////


        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index < 0)
                return false;
            this.RemoveAt(index);
            return true;
        }
        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)this.m_size)
                ThrowHelper.ThrowArgumentOutOfRangeException();
            --this.m_size;
            if (index < this.m_size)
                MarshalArrayBase.Copy(this.m_items, index + 1, this.m_items, index, this.m_size - index);
            this.m_items[this.m_size] = default(T);
            ++this.m_version;
        }
        public void RemoveRange(int index, int count)
        {
            if (index < 0 || count < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(index < 0 ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (this.m_size - index < count)
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            if (count <= 0)
                return;
            this.m_size -= count;
            if (index < this.m_size)
                MarshalArrayBase.Copy(this.m_items, index + count, this.m_items, index, this.m_size - index);

            //MarshalArrayBase.Clear(this.m_items, this.m_size, count);

            ++this.m_version;
        }
        public void Clear()
        {
            //因为是值类型，且非托管
            this.m_size = 0;
            ++this.m_version;
        }



        #endregion

        #region ////CopyTo////

        public void CopyTo(T[] array, int arrayIndex)
        {
            MarshalArrayBase.Copy<T>(this.m_items, 0, array, arrayIndex, this.m_size);
        }
        public void CopyTo(Array array, int index)
        {
            MarshalArrayBase.Copy(this.m_items, 0, (T[])array, index, this.m_size);
        }
        public void CopyTo(MarshalArray<T> array)
        {
            this.CopyTo(array, 0);
        }
        public void CopyTo(int index, MarshalArray<T> array, int arrayIndex, int count)
        {
            if (this.m_size - index < count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            }
            MarshalArrayBase.Copy(this.m_items, index, array, arrayIndex, count);
        }
        public void CopyTo(MarshalArray<T> array, int arrayIndex)
        {
            MarshalArrayBase.Copy(this.m_items, 0, array, arrayIndex, this.m_size);
        }


        #endregion

        #region ////IndexOf////

        public int IndexOf(T item)
        {
            return MarshalArrayBase.IndexOf<T>(this.m_items, item, 0, this.m_size);
        }
        public int IndexOf(T item, int index)
        {
            if (index > this.m_size)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            return MarshalArrayBase.IndexOf<T>(this.m_items, item, index, this.m_size - index);
        }
        public int IndexOf(T item, int index, int count)
        {
            if (index > this.m_size)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            if (count < 0 || index > this.m_size - count)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            return MarshalArrayBase.IndexOf<T>(this.m_items, item, index, count);
        }
        public int LastIndexOf(T item)
        {
            return this.LastIndexOf(item, this.m_size - 1, this.m_size);
        }
        public int LastIndexOf(T item, int index)
        {
            if (index >= this.m_size)
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            return this.LastIndexOf(item, index, index + 1);
        }
        public int LastIndexOf(T item, int index, int count)
        {
            if (this.m_size == 0)
                return -1;
            if (index < 0 || count < 0)
                ThrowHelper.ThrowArgumentOutOfRangeException(index < 0 ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (index >= this.m_size || count > index + 1)
                ThrowHelper.ThrowArgumentOutOfRangeException(index >= this.m_size ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
            return MarshalArrayBase.LastIndexOf<T>(this.m_items, item, index, count);
        }


        #endregion


        //--------------------------------------------------------------------
        #region ////工具函数////
        //--------------------------------------------------------------------

        private static bool IsCompatibleObject(object value)
        {
            return value is T || value == null && !typeof(T).IsValueType;
        }

        private static void VerifyValueType(object value)
        {
            if (IsCompatibleObject(value))
                return;
            ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
        }
        #endregion


        //--------------------------------------------------------------------
        //枚举器
        //--------------------------------------------------------------------
        struct Enumerator : IEnumerator<T>
        {
            private MarshalList<T> list;
            private int index;
            private int version;
            private T current;

            internal Enumerator(MarshalList<T> list)
            {
                this.list = list;
                this.index = 0;
                this.version = list.m_version;
                this.current = default(T);
            }


            public void Dispose()
            {
            }


            public bool MoveNext()
            {
                MarshalList<T> list = this.list;

                if (this.version != list.m_version || (uint)this.index >= (uint)list.m_size)
                {
                    return this.MoveNextRare();
                }

                this.current = list.m_items[this.index];
                ++this.index;
                return true;
            }

            private bool MoveNextRare()
            {
                if (this.version != this.list.m_version)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }

                this.index = this.list.m_size + 1;
                this.current = default(T);
                return false;
            }


            public T Current { get { return this.current; } }

            object IEnumerator.Current
            {
                get
                {
                    if (this.index == 0 || this.index == this.list.m_size + 1)
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return (object)this.Current;
                }
            }

            void IEnumerator.Reset()
            {
                if (this.version != this.list.m_version)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }
                this.index = 0;
                this.current = default(T);
            }
        }
    }
}