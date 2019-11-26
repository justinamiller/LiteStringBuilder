using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringHelper
{
    //internal class ArrayPool<T> 
    //{
    //    public 
    //    readonly int _bufferLength;
    //    readonly object _gate;
    //    int _index;
    //    public static ArrayPool<T> Shared = new ArrayPoola<T>(10);

    //    public ArrayPool(int bufferLength)
    //    {
    //        this._bufferLength = bufferLength;
    //        this.buffers = new T[4][];
    //        this._gate = new object();
    //    }

    //    public T[] Rent()
    //    {
    //        lock (_gate)
    //        {
    //            if (_index >= buffers.Length)
    //            {
    //                Array.Resize(ref buffers, buffers.Length * 2);
    //            }

    //            if (buffers[_index] == null)
    //            {
    //                buffers[_index] = new T[_bufferLength];
    //            }

    //            var buffer = buffers[_index];
    //            buffers[_index] = null;
    //            _index++;

    //            return buffer;
    //        }
    //    }

    //    public void Return(T[] array)
    //    {
    //        if (array.Length != _bufferLength)
    //        {
    //            throw new InvalidOperationException("return buffer is not from pool");
    //        }

    //        lock (_gate)
    //        {
    //            if (_index != 0)
    //            {
    //                buffers[--_index] = array;
    //            }
    //        }
    //    }
    //}
}
