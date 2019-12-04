using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

[assembly: InternalsVisibleTo("Benchmark")]
[assembly: InternalsVisibleTo("LiteStringBuilder.Tests")]
namespace StringHelper
{
    internal sealed class SimpleArrayPool<T>
    {
        private static T[] s_emptyArray;
        private readonly Container[] _containers;
        

        internal SimpleArrayPool()
        {
            int containerCount = 17;
            Container[] container = new Container[containerCount];
            for (int index = 0; index < containerCount; ++index)
            {
                container[index] = new Container(16 << index, Environment.ProcessorCount * 5);
            }
            this._containers = container;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int SelectContainerIndex(int bufferSize)
        {
            uint num1 = (uint)(bufferSize - 1) >> 4;
            int num2 = 0;
            if (num1 > ushort.MaxValue)
            {
                num1 >>= 16;
                num2 = 16;
            }
            if (num1 > byte.MaxValue)
            {
                num1 >>= 8;
                num2 += 8;
            }
            if (num1 > 15U)
            {
                num1 >>= 4;
                num2 += 4;
            }
            if (num1 > 3U)
            {
                num1 >>= 2;
                num2 += 2;
            }
            if (num1 > 1U)
            {
                num1 >>= 1;
                ++num2;
            }
            return num2 + (int)num1;
        }

        public  T[] Rent(int minimumLength)
        {
            if (minimumLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLength));
            }
            if (minimumLength == 0)
            {
                return s_emptyArray ?? (s_emptyArray = Array.Empty<T>());
            }

            int index1 =SelectContainerIndex(minimumLength);
            int containerLength = _containers.Length;
  
            if (index1 < containerLength)
            {
                Container[] container = _containers;
                int index2 = index1;
                do
                {
                    T[] objArray2 = container[index2].Rent();
                    if (objArray2 != null)
                    {
                        return objArray2;
                    }
                }
                while (++index2 < containerLength && index2 != index1 + 2);

                //unable to find open container;create one base on length of orginal container
                return new T[container[index1]._bufferLength];
            }


            //outside container capacity or unable to find open instance; need to create a new instance.
            return new T[minimumLength];
        }

        public  void Return(T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            int length = array.Length;
            if (length == 0)
            {
                //empty length no need to add
                return;
            }
            int index = SelectContainerIndex(length);
            if (index < _containers.Length)
            {
                this._containers[index].Return(array);
            }
        }


        private sealed class Container
        {
            internal readonly int _bufferLength;
            private readonly T[][] _buffers;
            private SpinLock _lock;
            private int _index;

            internal Container(int bufferLength, int numberOfBuffers)
            {
                //do not track; enableThreadOwnerTracking
                this._lock = new SpinLock(false);
                this._buffers = new T[numberOfBuffers][];
                this._bufferLength = bufferLength;
            }
#if !NETSTANDARD1_3
            [ExcludeFromCodeCoverage]
#endif
            public override string ToString()
            {
#if DEBUG
                return $"Index: {_index} Pooled: {_buffers.Where(b => b != null).Count()}/{_buffers.Length} Length: {_bufferLength}";
#else
                return base.ToString();
#endif
            }

            internal T[] Rent()
            {
                T[][] buffers = this._buffers;
                T[] objArray = null;
                bool lockTaken = false;
                try
                {
                    this._lock.Enter(ref lockTaken);
                    if (this._index < buffers.Length)
                    {
                        objArray = buffers[this._index];
                        buffers[this._index++] = null;
                    }
                    else
                    {
                        //pool has been flush; return null;
                        return null;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        this._lock.Exit(false);
                    }
                }

                if (objArray is null)
                {
                    //create new pooled object 
                    return new T[this._bufferLength];
                }
                //return pooled object.
                return objArray;
            }

            internal void Return(T[] array)
            {
                if (array.Length != this._bufferLength) 
                {
                    throw new ArgumentException("BufferNotFromPool", nameof(array));
                }
        
                bool lockTaken = false;
                try
                {
                    this._lock.Enter(ref lockTaken);
                    if (this._index != 0)
                    {
                        this._buffers[--this._index] = array;
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        this._lock.Exit(false);
                    }
                }
            }
        }
    }
}