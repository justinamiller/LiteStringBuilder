﻿using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Buffers;

[assembly: InternalsVisibleTo("Benchmark")]
[assembly: InternalsVisibleTo("LiteStringBuilder13.Tests")]
namespace StringHelper
{
    ///<summary>
    /// Mutable String class, optimized for speed and memory allocations while retrieving the final result as a string.
    /// Similar use than StringBuilder, but avoid a lot of allocations done by StringBuilder (conversion of int and float to string, frequent capacity change, etc.)
    ///</summary>
    public class LiteStringBuilder13
    {
        ///<summary>Working mutable string</summary>
        private char[] _buffer = null;
        private int _bufferPos = 0;
        private const int DefaultCapacity = 16;
        private readonly static char[] _charNumbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private readonly static CultureInfo s_Culture = CultureInfo.CurrentCulture;
        private int _totalOffset = 0;

        private  Bucket[] _buckets = Array.Empty<Bucket>();
        private int _bucketIndex = 0;
#pragma warning disable HAA0501 // Explicit new array type allocation
        private readonly static char[][] s_bool = new char[2][]
#pragma warning restore HAA0501 // Explicit new array type allocation
  {
            new char[]{ 'F','a','l','s','e'},
            new char[]{ 'T', 'r','u','e' }
  };


        /// <summary>
        /// gets the size of data in the buffer pool
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _totalOffset + _bufferPos;
            }
        }

      

        /// <summary>
        /// Get a new instance of LiteStringBuilder13
        /// </summary>
        /// <param name="initialCapacity"></param>
        /// <returns></returns>
        public static LiteStringBuilder13 Create(int initialCapacity = DefaultCapacity)
        {
            return new LiteStringBuilder13(initialCapacity);
        }

        public LiteStringBuilder13(int initialCapacity = DefaultCapacity)
        {
            _buffer = new char[initialCapacity > 0 ? initialCapacity : DefaultCapacity];//Pool_Instance.Rent(capacity);
        }

        public LiteStringBuilder13(string value)
        {

            if (value != null)
            {
                int capacity = value.Length > 0 ? value.Length : DefaultCapacity;
                _buffer = new char[capacity];
                this.Append(value);
            }
            else
            {
                _buffer = new char[DefaultCapacity];
            }
        }


        public bool IsEmpty()
        {
            return this.Length == 0;
        }

#if NETCOREAPP3_0 || NETSTANDARD2_1
        private static readonly SpanAction<char, ValueTuple<Bucket[], int, char[], int>> StringCreationAction = (span, ctx) =>
        {
            var buckets = ctx.Item1;
            var bucketLength = ctx.Item2;
            var buffer = ctx.Item3;
            var pos = ctx.Item4;

            int position = 0;

            for (var i = 0; i < bucketLength; i++)
            {
                var bucket = buckets[i];
                bucket.Span.TryCopyTo(position > 0 ? span.Slice(position) : span);
                position += bucket.Length;
            }

            if (pos > 0)
            {
                new Span<char>(buffer, 0, pos).TryCopyTo(position > 0 ? span.Slice(position) : span);
            }
        };
#else
        private static string AllocateString(int length)
        {
            return new string('\0', length);
        }

        private readonly static Func<int, string> FastAllocateString;
        static LiteStringBuilder13()
        {
            MethodInfo fasMethod = typeof(string).GetMethod("FastAllocateString", BindingFlags.NonPublic | BindingFlags.Static);
            if (fasMethod is null)
            {
                fasMethod = typeof(LiteStringBuilder13).GetMethod("AllocateString", BindingFlags.NonPublic | BindingFlags.Static);
            }

            FastAllocateString = (Func<int, string>)fasMethod.CreateDelegate(typeof(Func<int, string>));
        }
#endif

        public override string ToString()
        {
            int totalLength = Length;
            if (totalLength == 0)
            {
                return string.Empty;
            }
#if NETCOREAPP3_0 || NETSTANDARD2_1
            return string.Create(totalLength, (_buckets, _bucketIndex, _buffer, _bufferPos), StringCreationAction);
#else
            int pos = _bufferPos;
            int bucketLength = _bucketIndex;
            var buckets = _buckets;
            int size;

            string allocString = FastAllocateString(totalLength);

            unsafe
            {
                fixed (char* destPtr = allocString)
                {
                    var ptr = (byte*)&destPtr[0];
                    for (var i = 0; i < bucketLength; i++)
                    {
                        var buffer = buckets[i];
                        size = buffer.Length * 2;
                        fixed (char* sourcePtr = &buffer.Buffer[0])
                        {
                            Buffer.MemoryCopy((byte*)sourcePtr, ptr, size, size);
                        }
                        ptr += size;
                    }

                    if (pos > 0)
                    {
                        size = pos * 2;
                        fixed (char* sourcePtr = &_buffer[0])
                        {
                            Buffer.MemoryCopy((byte*)sourcePtr, ptr, size, size);
                        }
                    }
                }
            }
            return allocString;
#endif
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LiteStringBuilder13);
        }
        public bool Equals(LiteStringBuilder13 other)
        {
            // Check for null
            if (other is null)
            {
                return false;
            }


            // Check for same reference
            if (ReferenceEquals(this, other))
            {
                return true;
            }
           

            // Check for same Id and same Values
            if (other.Length != this.Length)
            {
                return false;
            }

            return this.ToString() == other.ToString();
        }

        // Set methods: 

        ///<summary>Set a string, no memorry allocation</summary>
        public void Set(string str)
        {
            // We fill the _chars list to manage future appends, but we also directly set the final stringGenerated
            Clear();
            Append(str);
        }

        ///<summary>Clear values, and append new values. Will allocate a little memory due to boxing</summary>
        public void Set(params object[] values)
        {
            Clear();

            for (int i = 0; i < values.Length; i++)
            {
                Append(values[i]);
            }
        }


        ///<summary>Reset the string to empty</summary>
        public LiteStringBuilder13 Clear()
        {
            _totalOffset = 0;
            _buckets = Array.Empty<Bucket>();
            _bufferPos = 0;
            return this;
        }

        ///<summary>Will allocate on the array creatation, and on boxing values</summary>
        public LiteStringBuilder13 Append(params object[] values)
        {
            if (values != null)
            {
                int len = values.Length;
                for (var i = 0; i < len; i++)
                {
                    this.Append<object>(values[i]);
                }
            }
            return this;
        }

        ///<summary>Will allocate on the array creatation</summary>
        public LiteStringBuilder13 Append<T>(params T[] values)
        {

            if (values != null)
            {
                //Type type = typeof(T);
                //var value = (T)Convert.ChangeType(values[i], type);

                int len = values.Length;
                for (var i = 0; i < len; i++)
                {
                    Append(values[i]);
                }
            }
            return this;
        }

        private void Append<T>(T value)
        {
            if (value == null)
            {
                //null no need to add
                return;
            }
            if (value is string)
            {
                this.Append(value as string);
            }
            else if (value is char)
            {
                this.Append((char)(object)value);
            }
            else if (value is char[])
            {
                this.Append((char[])(object)value);
            }
            else if (value is int)
            {
                this.Append((int)(object)value);
            }
            else if (value is long)
            {
                this.Append((long)(object)value);
            }
            else if (value is bool)
            {
                this.Append((bool)(object)value);
            }
            else if (value is DateTime)
            {
                this.Append((DateTime)(object)value);
            }
            else if (value is decimal)
            {
                this.Append((decimal)(object)value);
            }
            else if (value is float)
            {
                this.Append((float)(object)value);
            }
            else if (value is double)
            {
                this.Append((double)(object)value);
            }
            else if (value is byte)
            {
                this.Append((byte)(object)value);
            }
            else if (value is sbyte)
            {
                this.Append((sbyte)(object)value);
            }
            else if (value is ulong)
            {
                this.Append((ulong)(object)value);
            }
            else if (value is uint)
            {
                this.Append((uint)(object)value);
            }
            else
            {
                //default handling is tostring()
                this.Append(value.ToString());
            }
        }

        ///<summary>Append a string without memory allocation</summary>
        public LiteStringBuilder13 Append(string value)
        {
            if (value != null)
            {
                int length = value.Length;
                if (length > 0)
                {
                    EnsureCapacity(length);
                    int pos = _bufferPos;
                    int bytesSize = length * 2;
                    unsafe
                    {
                        fixed (char* valuePtr = value)
                        fixed (char* destPtr = &_buffer[pos])
                        {
                            Buffer.MemoryCopy(valuePtr, destPtr, bytesSize, bytesSize);
                        }
                    }

                    //    value.AsSpan().TryCopyTo(pos > 0 ? new Span<char>(buffer, pos, buffer.Length - pos) : new Span<char>(buffer));

                    _bufferPos += length;
                }
            }
            return this;
        }

        ///<summary>Generate a new line</summary>
        public LiteStringBuilder13 AppendLine()
        {
            return Append(Environment.NewLine);
        }

        ///<summary>Append a string and new line without memory allocation</summary>
        public LiteStringBuilder13 AppendLine(string value)
        {
            Append(value);
            return Append(Environment.NewLine);
        }

        ///<summary>Append a char without memory allocation</summary>
        public LiteStringBuilder13 Append(char value)
        {
            if (_bufferPos == this._buffer.Length)
            {
                EnsureCapacity(1);
            }

            _buffer[_bufferPos++] = value;
            return this;
        }




        ///<summary>Append a bool without memory allocation</summary>
        public LiteStringBuilder13 Append(bool value)
        {
            if (value)
            {
                //true
                return Append(s_bool[1]);
            }
            else
            {
                //false
                return Append(s_bool[0]);
            }
        }


        ///<summary>Append a char[] without memory allocation</summary>
        public LiteStringBuilder13 Append(char[] value)
        {
            if (value != null)
            {
                int length = value.Length;
                if (length > 0)
                {
                    EnsureCapacity(length);
                    // new Span<char>(value).TryCopyTo(new Span<char>(_buffer, _bufferPos, n));
                    Buffer.BlockCopy(value, 0, _buffer, _bufferPos * 2, length * 2);
                    _bufferPos += length;
                }
            }


            return this;
        }


        ///<summary>Append an object.ToString(), allocate some memory</summary>
        public LiteStringBuilder13 Append(object value)
        {
            if (value is null)
                return this;

            return Append(value.ToString());
        }

        ///<summary>Append an datetime with small memory allocation</summary>
        public LiteStringBuilder13 Append(DateTime value)
        {
            return Append(value.ToString(s_Culture));
        }


        ///<summary>Append an sbyte with some memory allocation</summary>
        public LiteStringBuilder13 Append(sbyte value)
        {
            if (value < 0)
            {
                return Append((ulong)-((int)value), true);
            }
            return Append((ulong)value, false);
        }


        ///<summary>Append an byte with some memory allocation</summary>
        public LiteStringBuilder13 Append(byte value)
        {
            return Append(value, false);
        }



        ///<summary>Append an uint without memory allocation</summary>
        public LiteStringBuilder13 Append(uint value)
        {
            return Append((ulong)value, false);
        }

        ///<summary>Append an ulong without memory allocation</summary>
        public LiteStringBuilder13 Append(ulong value)
        {
            return Append(value, false);
        }

        ///<summary>Append an int without memory allocation</summary>
        public LiteStringBuilder13 Append(short value)
        {
            return Append((int)value);
        }


        ///<summary>Append an int without memory allocation</summary>
        public LiteStringBuilder13 Append(int value)
        {
            bool isNegative = value < 0;
            if (isNegative)
            {
                value = -value;
            }
            return Append((ulong)value, isNegative);
        }

        public LiteStringBuilder13 Append(float value)
        {
            return Append(value.ToString(s_Culture));
        }

        public LiteStringBuilder13 Append(decimal value)
        {
            return Append(value.ToString(s_Culture));
        }


        public LiteStringBuilder13 Append(long value)
        {
            bool isNegative = value < 0;
            if (isNegative)
            {
                value = -value;
            }
            return Append((ulong)value, isNegative);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LiteStringBuilder13 Append(ulong value, bool isNegative)
        {
            // Allocate enough memory to handle any ulong number
            int length = Utilities.GetIntLength(value);


            EnsureCapacity(length + (isNegative ? 1 : 0));
            var buffer = _buffer;
            // Handle the negative case
            if (isNegative)
            {
                buffer[_bufferPos++] = '-';
            }
            if (value <= 9)
            {
                //between 0-9
                buffer[_bufferPos++] = _charNumbers[value];
                return this;
            }

            // Copy the digits with reverse in mind.
            _bufferPos += length;
            int nbChars = _bufferPos - 1;
            do
            {
                buffer[nbChars--] = _charNumbers[value % 10];
                value /= 10;
            } while (value != 0);

            return this;
        }

        ///<summary>Append a double without memory allocation.</summary>
        public LiteStringBuilder13 Append(double value)
        {
            return Append(value.ToString(s_Culture));
        }

#region OLDCODE
#if !NETSTANDARD1_6 && !NETCOREAPP1_1
        [ExcludeFromCodeCoverage]
#endif
        private LiteStringBuilder13 InternalAppend(float valueF)
        {
            double value = valueF;

            EnsureCapacity(32); // Check we have enough buffer allocated to handle any float number

            // Handle the 0 case
            if (value == 0)
            {
                _buffer[_bufferPos++] = '0';
                return this;
            }

            // Handle the negative case
            if (value < 0)
            {
                value = -value;
                _buffer[_bufferPos++] = '-';
            }

            // Get the 7 meaningful digits as a long
            int nbDecimals = 0;
            while (value < 1000000)
            {
                value *= 10;
                nbDecimals++;
            }
            long valueLong = (long)System.Math.Round(value);

            // Parse the number in reverse order
            int nbChars = 0;
            bool isLeadingZero = true;
            while (valueLong != 0 || nbDecimals >= 0)
            {
                // We stop removing leading 0 when non-0 or decimal digit
                if (valueLong % 10 != 0 || nbDecimals <= 0)
                    isLeadingZero = false;

                // Write the last digit (unless a leading zero)
                if (!isLeadingZero)
                    _buffer[_bufferPos + (nbChars++)] = (char)('0' + valueLong % 10);

                // Add the decimal point
                if (--nbDecimals == 0 && !isLeadingZero)
                    _buffer[_bufferPos + (nbChars++)] = '.';

                valueLong /= 10;
            }
            _bufferPos += nbChars;

            // Reverse the result
            for (int i = nbChars / 2 - 1; i >= 0; i--)
            {
                char c = _buffer[_bufferPos - i - 1];
                _buffer[_bufferPos - i - 1] = _buffer[_bufferPos - nbChars + i];
                _buffer[_bufferPos - nbChars + i] = c;
            }

            return this;
        }

#if !NETSTANDARD1_6 && !NETCOREAPP1_1
        [ExcludeFromCodeCoverage]
#endif
        private LiteStringBuilder13 InternalAppend(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return Append(value.ToString(s_Culture));
            }

            EnsureCapacity(45); // Check we have enough buffer allocated to handle most double number
            // Handle the 0 case
            if (value == 0)
            {
                _buffer[_bufferPos++] = '0';
                return this;
            }

            // Handle the negative case
            if (value < 0)
            {
                value = -value;
                _buffer[_bufferPos++] = '-';
            }

            // Get the meaningful digits as a long
            int nbDecimals = 0;
            //while (value < 1000000)
            while (value < 10000000000000)//14
            {
                value *= 10;
                nbDecimals++;
            }
            long valueLong = (long)Math.Round(value);

            int positiveNumber = valueLong > 0 ? 1 : -1;

            // Parse the number in reverse order
            int nbChars = 0;
            bool isLeadingZero = true;
            while (valueLong != 0 || nbDecimals >= 0)
            {
                // We stop removing leading 0 when non-0 or decimal digit
                if (valueLong % 10 != 0 || nbDecimals <= 0)
                    isLeadingZero = false;

                //check if needs have to grow more.
                if (_bufferPos + nbChars + 2 >= this._buffer.Length)
                {
                    EnsureCapacity(45);
                }


                // Write the last digit (unless a leading zero)
                if (!isLeadingZero)
                    _buffer[_bufferPos + (nbChars++)] = _charNumbers[(valueLong % 10) * positiveNumber];

                // Add the decimal point
                if (--nbDecimals == 0 && !isLeadingZero)
                    _buffer[_bufferPos + (nbChars++)] = '.';

                valueLong /= 10;
            }
            _bufferPos += nbChars;

            // Reverse the result
            for (int i = nbChars / 2 - 1; i >= 0; i--)
            {
                char c = _buffer[_bufferPos - i - 1];
                _buffer[_bufferPos - i - 1] = _buffer[_bufferPos - nbChars + i];
                _buffer[_bufferPos - nbChars + i] = c;
            }

            return this;
        }
        #endregion



        /// <summary>
        /// Finds the bucket that logically follows the 'bucket' bucket.  
        /// </summary>
        private Bucket Next(Bucket bucket)
        {
            return FindBucketForIndex(bucket.Offset + bucket.Length);
        }


        /// <summary>
        /// Finds the chunk for the logical index (number of characters in the whole stringbuilder) 'index'
        /// YOu can then get the offset in this chunk by subtracting the m_BlockOffset field from 'index' 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Bucket FindBucketForIndex(int index)
        {
            for (var i = 0; i < _bucketIndex; i++)
            {
                if (_buckets[i].Offset >= index)
                {
                    return _buckets[i];
                }
            }

            return new Bucket(_buffer, _bufferPos, this._totalOffset);
        }

        /// <summary>
        /// Returns true if the string that is starts at 'chunk' and 'indexInChunk, and has a logical
        /// length of 'count' starts with the string 'value'. 
        /// </summary>
        private bool StartsWith(Bucket chunk, int indexInChunk, int count, string oldValue, string newValue)
        {
            char firstChar = oldValue[0];
            int oldstrLength = oldValue.Length;
            int newstrLength = newValue.Length;
            bool isToReplace = false;
            int index = 0;
            int matchChunkIndex = 0;
            int[] matchChunks = new int[_bucketIndex + 1];
            int calculateOffset = 0;
            while (count > 0)
            {
                if (indexInChunk >= chunk.Length)
                {
                    chunk = Next(chunk);

                    indexInChunk = 0;
                }



                if (chunk.Buffer[indexInChunk] == firstChar) // If first character found, check for the rest of the string to replace
                {
                    matchChunkIndex = 0;
                    matchChunks[matchChunkIndex++] = chunk.Offset;

                    int k = 1;//skip one char
                    int i = 1;
                    while (k < oldstrLength)
                    {
                        if (indexInChunk + i >= chunk.Length)
                        {
                            chunk = Next(chunk);

                            indexInChunk = 0;
                            i = 0;
                            matchChunks[matchChunkIndex++] = chunk.Offset;
                        }


                        if (chunk.Buffer[indexInChunk + i] == oldValue[k])
                        {
                            i++;
                            k++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    isToReplace = (k == oldstrLength);
                }

                if (isToReplace)
                {
                    char[] newBuffer;
                    int newSize = 0;
                    for (var i = 0; i < matchChunkIndex; i++)
                    {
                        newSize += FindBucketForIndex(matchChunks[i]).Length;
                    }



                    newBuffer = new char[newSize + (newstrLength - oldstrLength)];
                    int copyIndex = 0;
                    Bucket buffer;

                    buffer = FindBucketForIndex(matchChunks[0]);

                    int localIndex = 0;
                    int skipIndex = 0;

                    int targetIndex = index;// + calculateOffset;// - (buffer.Offset - buffer.Length);

                    if (targetIndex > 0)
                    {
                        //copy front
                        new Span<char>(buffer.Buffer, 0, targetIndex).TryCopyTo(new Span<char>(newBuffer, 0, targetIndex));
                        copyIndex += targetIndex;
                        localIndex += targetIndex;
                        skipIndex += targetIndex;
                    }

                    //replace mid
                    for (int k = 0; k < newstrLength; k++)
                    {
                        newBuffer[copyIndex + k] = newValue[k];
                    }
                    copyIndex += newstrLength;
                    localIndex += newstrLength;



                    if (matchChunkIndex != 0)
                    {
                        for (var i = 1; i < matchChunkIndex; i++)
                        {
                            buffer = FindBucketForIndex(matchChunks[i]);
                            skipIndex += buffer.Length;
                        }
                    }

                    if ((buffer.Offset + buffer.Length) > skipIndex)
                    {
                        //copy remainding
                        int sourceIndex = buffer.Length - (buffer.Offset + buffer.Length - skipIndex);
                        new Span<char>(buffer.Buffer).Slice(sourceIndex).TryCopyTo(new Span<char>(newBuffer, targetIndex + newstrLength, newBuffer.Length - (targetIndex + newstrLength)));
                        copyIndex += (buffer.Length - (targetIndex + oldstrLength));
                    }




                    buffer = FindBucketForIndex(matchChunks[0]);

                    int bufferIndex = Array.IndexOf(_buckets, buffer);
                    if (bufferIndex == -1)
                    {
                        _buffer = newBuffer;
                        _bufferPos = copyIndex;
                    }
                    else
                    {
                        //new size modifier
                        _totalOffset = _totalOffset + newstrLength - oldstrLength;
                        if (matchChunkIndex != _bucketIndex)
                        {
                            _buckets[bufferIndex] = new Bucket(newBuffer, newBuffer.Length, buffer.Offset);
                            _bucketIndex = bufferIndex;
                        }
                        else
                        {
                            //all from bucket
                            _buckets[0] = new Bucket(newBuffer, newBuffer.Length, buffer.Offset);
                            _bucketIndex = 1;
                        }
                    }

                    calculateOffset += (newstrLength - oldstrLength);
                    isToReplace = false;

                }//end isToReplace



                --count;
                indexInChunk++;

                index++;

            }
            return true;
        }

        public LiteStringBuilder13 Replace(string oldStr, string newStr)
        {
            int totalLength = this.Length;
            if (totalLength == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0)
                return this;

            if (newStr == null)
                newStr = "";



            var chunk = FindBucketForIndex(0);
            if (StartsWith(chunk, 0, totalLength, oldStr, newStr))
            {

            }

            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int appendLength)
        {
            var buffer = _buffer;
            int currentCapacity = buffer.Length;
            int pos = _bufferPos;
            if (pos + appendLength > currentCapacity)
            {
                int totalLength = this.Length;
                int minCalc = (totalLength <= 8000) ? totalLength : 8000;
                int newCapacity = (appendLength > minCalc) ? appendLength : minCalc;
                if (pos > 0)
                {
                    //set size
                    EnsureBucketCapacity();

                    //copy data
                    _buckets[_bucketIndex++] = new Bucket(buffer, pos, _totalOffset);
                    _totalOffset += pos;
                    _bufferPos = 0;
                }

                _buffer = new char[newCapacity]; ;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureBucketCapacity()
        {
            int len = _buckets.Length;
            int index = _bucketIndex;
            if (len== index)
            {
                Bucket[] newItems = new Bucket[len + 4];
                if (index > 0)
                {

#if NETCOREAPP3_0 || NETCOREAPP1_0 || NETCOREAPP1_1 || NETCOREAPP2_0
                    new Span<Bucket>(_buckets, 0, index).CopyTo(new Span<Bucket>(newItems));
#else

                    //better performance for CLR
                   Array.Copy(_buckets, 0, newItems, 0, index);
#endif
                }
                _buckets = newItems;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 0;
                for (var i = 0; i < _bufferPos; i++)
                {
                    hash += _buffer[i];
                }
                return 31 * hash + this.Length;
            }
        }
    }
}