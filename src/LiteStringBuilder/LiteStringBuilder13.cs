using System.Collections.Generic;
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

        private Bucket[] _buckets = Array.Empty<Bucket>();
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
            return FindBucketForIndex(bucket.OffsetLength);
        }


        /// <summary>
        /// Finds the chunk for the logical index (number of characters in the whole stringbuilder) 'index'
        /// YOu can then get the offset in this chunk by subtracting the m_BlockOffset field from 'index' 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Bucket FindBucketForIndex(int index)
        {
            for (int i = 0; i < _bucketIndex; i++)
            {
                Bucket bucket = _buckets[i];
                if (bucket.Offset >= index)
                {
                    return bucket;
                }
            }

            return new Bucket(_buffer, _bufferPos, this._totalOffset);
        }

        private void ReplaceFast(Bucket chunk, int indexInChunk, int count, string oldValue, string newValue)
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
        }

        private LiteStringBuilder13 ReplaceSlow(string oldStr, string newStr)
        {

            int totalLength = this.Length;
            if (totalLength == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0 || oldstrLength > totalLength)
                return this;

            if (newStr == null)
                newStr = string.Empty;

            var value = this.ToString();
            if (s_Culture.CompareInfo.IndexOf(value, oldStr, CompareOptions.None) >= 0)
            {

                int bucketIndex = _bucketIndex;
                if (bucketIndex == 0)
                {
                    EnsureBucketCapacity();
                }
                else if (bucketIndex > 1)
                {
                    for (var i = 1; i < bucketIndex; i++)
                    {
                        _buckets[i] = default;
                    }
                }

                var replaceChar = value.Replace(oldStr, newStr).ToCharArray();
                int newLen = replaceChar.Length;
                _buckets[0] = new Bucket(replaceChar, newLen, 0);

                _bucketIndex = 1;
                _bufferPos = 0;
                _totalOffset = newLen;
            }


            //var chunk = FindBucketForIndex(0);
            //ReplaceFast(chunk, 0, totalLength, oldStr, newStr);

            return this;
        }

        public LiteStringBuilder13 Replace(string oldStr, string newStr)
        {
            int totalLength = this.Length;
            if (totalLength == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0 || oldstrLength > totalLength)
                return this;

            if (newStr == null)
                newStr = string.Empty;

            var bucket = FindBucketForIndex(0);
            int matchIndex = 0;

            int[] matching = Array.Empty<int>();
            int matchingIndex = 0;
            for (var i = 0; i < totalLength; i++)
            {
                if (i >= bucket.OffsetLength)
                {
                    bucket = Next(bucket);
                }

                if (bucket.Buffer[i - bucket.Offset] == oldStr[matchIndex])
                {
                    matchIndex++;

                    if (matchIndex == oldstrLength)
                    {
                        int matchingLength = matching.Length;
                        if (matchingLength == matchingIndex)
                        {
                            if (matchingLength == 0)
                            {
                                matching = new int[4];
                            }
                            else
                            {
                                Array.Resize(ref matching, matchingLength + 4);
                            }
                        }

                        //have complete match.
                        matching[matchingIndex++] = i - oldstrLength;
                        matchIndex = 0;
                    }
                }
                else
                {
                    //no match
                    matchIndex = 0;
                }
            }

            if (matchingIndex > 0)
            {
                int newStrLength = newStr.Length;
                bucket = default;
                int deltaLength = oldstrLength > newStrLength ? oldstrLength - newStrLength : newStrLength - oldstrLength;
                int size = ((totalLength / oldstrLength) * (oldstrLength + deltaLength)) + 1;
                var replacementChars = new char[size];
                for (var i = 0; i < matchingIndex; i++)
                {
                    if (i >= bucket.OffsetLength)
                    {
                        bucket = Next(bucket);
                    }



                }
            }


            return this;
        }

        public LiteStringBuilder13 Replace5(string oldStr, string newStr)
        {
            int totalLength = this.Length;
            if (totalLength == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0 || oldstrLength > totalLength)
                return this;

            if (newStr == null)
                newStr = string.Empty;

            var bucket = FindBucketForIndex(0);
            int matchIndex = 0;

            int[] matching = Array.Empty<int>();
            int matchingIndex = 0;
            char[] replaceChars = Array.Empty<char>();
            int replaceIndex = 0;
            for (var i = 0; i < totalLength; i++)
            {
                if (i >= bucket.Length + bucket.Offset)
                {
                    bucket = Next(bucket);
                }

                if (bucket.Buffer[i - bucket.Offset] == oldStr[matchIndex])
                {
                    matchIndex++;

                    if (matchIndex == oldstrLength)
                    {

                        //if (size>replaceChars.Length)
                        //{
                        //    if (replaceIndex == 0)
                        //    {
                        //        replaceChars = new char[size];
                        //    }
                        //    else
                        //    {
                        //        Array.Resize(ref replaceChars, size);
                        //    }
                        //}

                        int firstIndex = (i + 1) - bucket.Offset - oldstrLength;
                        Bucket first = bucket;
                        if (i >= bucket.Length + bucket.Offset)
                        {
                            first = FindBucketForIndex(i - oldstrLength);
                        }

                        if (first.Equals(bucket))
                        {
                            int size = bucket.Length + (newStr.Length - oldstrLength);
                            char[] newBuffer = new char[size];
                            int trackIndex = firstIndex;
                            if (firstIndex > 0)
                            {
                                Array.Copy(bucket.Buffer, 0, newBuffer, 0, firstIndex);
                            }

                            for (var a = 0; a < newStr.Length; a++)
                            {
                                newBuffer[trackIndex++] = newStr[a];
                            }

                            Array.Copy(bucket.Buffer, firstIndex + oldstrLength, newBuffer, trackIndex, bucket.Length - firstIndex - oldstrLength);


                        }
                        else
                        {

                        }

                        int matchingLength = matching.Length;
                        if (matchingLength == matchingIndex)
                        {
                            if (matchingLength == 0)
                            {
                                matching = new int[4];
                            }
                            else
                            {
                                Array.Resize(ref matching, matchingLength + 4);
                            }
                        }

                        //have complete match.
                        matching[matchingIndex++] = i - oldstrLength;
                        matchIndex = 0;
                    }
                }
                else
                {
                    //no match
                    matchIndex = 0;
                }
            }

            //if (matchingIndex > 0)
            //{
            //    return ReplaceSlow(oldStr, newStr);
            //}


            return this;
        }

        public LiteStringBuilder13 Replace2(string oldStr, string newStr)
        {
            int totalLength = this.Length;
            if (totalLength == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0 || oldstrLength > totalLength)
                return this;

            if (newStr == null)
                newStr = string.Empty;

            var bucket = FindBucketForIndex(0);
            int bucketLength = bucket.Length + bucket.Offset;
            int matchIndex = 0;

            int[] matching = Array.Empty<int>();
            int matchingIndex = 0;
            int matchingLength = 0;
            for (var i = 0; i < totalLength; i++)
            {

                if (i >= bucketLength)
                {
                    bucket = FindBucketForIndex(bucketLength);
                    bucketLength = bucket.Length + bucket.Offset;
                }

                if (bucket.Buffer[i - bucket.Offset] == oldStr[matchIndex])
                {
                    matchIndex++;
                    //do we match everything
                    if (matchIndex == oldstrLength)
                    {
                        if (matchingLength == matchingIndex)
                        {
                            if (matchingLength == 0)
                            {
                                matching = new int[4];
                                matchingLength = 4;
                            }
                            else
                            {
                                matchingLength += 4;
                                Array.Resize(ref matching, matchingLength);
                            }
                        }

                        //have complete match.
                        matching[matchingIndex++] = i - oldstrLength;
                        matchIndex = 0;
                    }//end full match
                }
                else
                {
                    //no match
                    matchIndex = 0;
                }
            }

            //if (matchingIndex > 0)
            //{
            //    return ReplaceSlow(oldStr, newStr);
            //}


            return this;
        }


        public LiteStringBuilder13 Replace4(string oldStr, string newStr)
        {
            return this;
        }

        public LiteStringBuilder13 Replace3(string oldStr, string newStr)
        {
            int totalLength = this.Length;
            if (totalLength == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0 || oldstrLength > totalLength)
                return this;

            if (newStr == null)
                newStr = string.Empty;

            int matchIndex = 0;
            int[] matching = Array.Empty<int>();
            int matchingIndex = 0;
            int matchingLength = 0;
            var bucket = FindBucketForIndex(0);
            int offset = bucket.Offset;
            int bucketLength = bucket.Length + offset;
            var buffer = bucket.Buffer;

            for (var i = 0; i < totalLength; i++)
            {
                if (i >= bucketLength)
                {
                    bucket = FindBucketForIndex(bucketLength);
                    offset = bucket.Offset;
                    bucketLength = bucket.Length + offset;
                    buffer = bucket.Buffer;
                }

                if (buffer[i - offset] == oldStr[matchIndex])
                {
                    matchIndex++;
                    //do we match everything
                    if (matchIndex == oldstrLength)
                    {
                        if (matchingLength == matchingIndex)
                        {
                            if (matchingLength == 0)
                            {
                                matching = new int[4];
                                matchingLength = 4;
                            }
                            else
                            {
                                Array.Resize(ref matching, matchingLength + 4);
                                matchingLength += 4;
                            }
                        }

                        //have complete match.
                        matching[matchingIndex++] = i - oldstrLength;
                        matchIndex = 0;
                    }//end full match
                }
                else
                {
                    //no match
                    matchIndex = 0;
                }
            }

            //if (matchingIndex > 0)
            //{
            //    return ReplaceSlow(oldStr, newStr);
            //}


            return this;
        }

        public LiteStringBuilder13 Replace_REAL(string oldStr, string newStr)
        {
            int totalLength = this.Length;
            if (totalLength == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0 || oldstrLength > totalLength)
                return this;

            if (newStr == null)
                newStr = string.Empty;

            int matchIndex = 0;
            int[] matching = Array.Empty<int>();
            int matchingIndex = 0;
            int matchingLength = 0;
            var bucket = FindBucketForIndex(0);
            int offset = bucket.Offset;
            int bucketLength = bucket.Length + offset;
            var buffer = bucket.Buffer;

            for (var i = 0; i < totalLength; i++)
            {
                if (i >= bucketLength)
                {
                    bucket = FindBucketForIndex(bucketLength);
                    offset = bucket.Offset;
                    bucketLength = bucket.Length + offset;
                    buffer = bucket.Buffer;
                }

                if (buffer[i - offset] == oldStr[matchIndex])
                {
                    matchIndex++;
                    //do we match everything
                    if (matchIndex == oldstrLength)
                    {
                        if (matchingLength == matchingIndex)
                        {
                            if (matchingLength == 0)
                            {
                                matching = new int[4];
                                matchingLength = 4;
                            }
                            else
                            {
                                Array.Resize(ref matching, matchingLength + 4);
                                matchingLength += 4;
                            }
                        }

                        //have complete match.
                        matching[matchingIndex++] = (i + 1) - oldstrLength;
                        matchIndex = 0;
                    }//end full match
                }
                else
                {
                    //no match
                    matchIndex = 0;
                }
            }


            if (matchingIndex > 0)
            {

                var value = this.ToString();
                int bucketIndex = _bucketIndex;
                if (bucketIndex == 0)
                {
                    EnsureBucketCapacity();
                }

                var newSize = totalLength + (bucketIndex * (newStr.Length - oldstrLength));
                var newBuffer = new char[newSize];
                bucket = default;

                var replace = value.Replace(oldStr, newStr);

                offset = matching[0];
                int matchMoveIndex = 1;
                int captureIndex = 0;
                for (var i = 0; i < newSize; i++)
                {
                    if (offset == i)
                    {
                        for (var x = 0; x < newStr.Length; x++)
                        {
                            newBuffer[(captureIndex++) + x] = newStr[x];
                        }
                        i += (oldstrLength - 1);
                        offset = matching[matchMoveIndex++];
                    }
                    else
                    {
                        newBuffer[captureIndex++] = value[i];
                    }
                }


                if (_bucketIndex == 0)
                {
                    EnsureBucketCapacity();
                }

                _buckets[0] = new Bucket(newBuffer, newSize, 0);

                _bucketIndex = 1;
                _bufferPos = 0;
                _totalOffset = newSize;

            }


            return this;
        }

        public LiteStringBuilder13 Replace_Slow(string oldStr, string newStr)
        {
            int totalLength = this.Length;
            if (totalLength == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0 || oldstrLength > totalLength)
                return this;

            if (newStr == null)
                newStr = string.Empty;

            int matchIndex = 0;
            var bucket = FindBucketForIndex(0);
            int offset = bucket.Offset;
            int bucketLength = bucket.Length + offset;
            var buffer = bucket.Buffer;

            for (var i = 0; i < totalLength; i++)
            {
                if (i >= bucketLength)
                {
                    bucket = FindBucketForIndex(bucketLength);
                    offset = bucket.Offset;
                    bucketLength = bucket.Length + offset;
                    buffer = bucket.Buffer;
                }

                if (buffer[i - offset] == oldStr[matchIndex])
                {
                    matchIndex++;
                    //do we match everything
                    if (matchIndex == oldstrLength)
                    {
                        //full match
                        var value = this.ToString();
                        int bucketIndex = _bucketIndex;
                        if (bucketIndex == 0)
                        {
                            EnsureBucketCapacity();
                        }


                        var replace = value.Replace(oldStr, newStr);
                        _buckets[0] = new Bucket(replace.ToCharArray(), replace.Length, 0);

                        _bucketIndex = 1;
                        _bufferPos = 0;
                        _totalOffset = replace.Length;

                        return this;
                    }//end full match
                }
                else
                {
                    //no match
                    matchIndex = 0;
                }
            }

            return this;
        }

        ///<summary>Replace all occurences of a string by another one</summary>
        public LiteStringBuilder13 Replace_Try(string oldStr, string newStr)
        {
            int length = this.Length;


            if (length == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0)
                return this;

            if (newStr == null)
                newStr = "";




            int index = 0;
            char[] replacementChars = null;
            int replaceIndex = 0;

            char firstChar = oldStr[0];
            var bucket = FindBucketForIndex(0);
            int newStrLength = newStr.Length;

            // Create the new string into _replacement
            for (int i = 0; i < length; i++)
            {
                bool isToReplace = false;
                if (i >= bucket.OffsetLength)
                {
                    bucket = FindBucketForIndex(bucket.OffsetLength);

                }

                if (bucket.Buffer[i - bucket.Offset] == firstChar) // If first character found, check for the rest of the string to replace
                {
                    int k = 1;//skip one char

                    if (i + k >= bucket.OffsetLength)
                    {
                        bucket = FindBucketForIndex(bucket.OffsetLength);
                    }


                    while (k < oldstrLength && bucket.Buffer[i + k - bucket.Offset] == oldStr[k])
                    {
                        k++;

                        if (i + k >= bucket.OffsetLength)
                        {
                            bucket = FindBucketForIndex(bucket.OffsetLength);

                        }

                    }
                    isToReplace = (k == oldstrLength);
                }
                if (isToReplace) // Do the replacement
                {

                    if (replaceIndex == 0)
                    {
                        int deltaLength = oldstrLength > newStrLength ? oldstrLength : newStrLength;
                        int size = ((length / oldstrLength) * deltaLength);

                        //first replacement target
                        //  replacementChars = ArrayPool<char>.Shared.Rent(size);
                        replacementChars = new char[size];
                        //copy first set of char that did not match.
                        //  Buffer.BlockCopy(_buffer, 0, replacementChars, 0, i * 2);
                        //   value.TryCopyTo(_buffer.AsSpan(_bufferPos, _charsCapacity - _bufferPos));
                        var buffer = FindBucketForIndex(0);
                        for (var a = 0; a < i; a++)
                        {
                            if (a >= buffer.OffsetLength)
                            {
                                buffer = FindBucketForIndex(buffer.OffsetLength);
                            }

                            replacementChars[a] = buffer.Buffer[a - buffer.Offset];
                        }


                        // new Span<char>(_buffer, 0, i).TryCopyTo(new Span<char>(replacementChars, 0, i));
                        index = i;
                    }

                    replaceIndex++;
                    i += oldstrLength - 1;
                    for (int k = 0; k < newStrLength; k++)
                    {
                        replacementChars[index++] = newStr[k];
                    }
                }
                else if (replaceIndex > 0)// No replacement, copy the old character
                {
                    //could batch these up instead one at a time!
                    replacementChars[index++] = bucket.Buffer[i - bucket.Offset];
                }
            }//end for

            if (replaceIndex > 0)
            {
                // Copy back the new string into _chars
                // EnsureCapacity(index - _bufferPos);
                // new Span<char>(replacementChars, 0, index).TryCopyTo(new Span<char>(_buffer));
                if (_bucketIndex == 0)
                {
                    EnsureBucketCapacity();
                }

                _buckets[0] = new Bucket(replacementChars, index, 0);
                _totalOffset = index;
                _bucketIndex = 1;

                //ArrayPool<char>.Shared.Return(replacementChars);
                // _bufferPos = index;
                // _bucketIndex = 0;
                _bufferPos = 0;
            }


            return this;
        }

        public LiteStringBuilder13 Replace_TryCache(string oldStr, string newStr)
        {
            int length = this.Length;


            if (length == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0)
                return this;

            if (newStr == null)
                newStr = "";

            int newStrLength = newStr.Length;


            int index = 0;
            char[] replacementChars = null;

            char firstChar = oldStr[0];
            var bucket = FindBucketForIndex(0);
            bool isToReplace = false;
            // Create the new string into _replacement
            for (int i = 0; i < length; i++)
            {
                if (i >= bucket.OffsetLength)
                {
                    bucket = FindBucketForIndex(bucket.OffsetLength);
                }

                char bucketChar = bucket.Buffer[i - bucket.Offset];
                if (bucketChar == firstChar) // If first character found, check for the rest of the string to replace
                {
                    int k = 1;//skip one char

                    if (i + k >= bucket.OffsetLength)
                    {
                        bucket = FindBucketForIndex(bucket.OffsetLength);
                    }


                    while (k < oldstrLength && bucket.Buffer[i + k - bucket.Offset] == oldStr[k])
                    {
                        k++;

                        if (i + k >= bucket.OffsetLength)
                        {
                            bucket = FindBucketForIndex(bucket.OffsetLength);
                        }
                    }

                    isToReplace = (k == oldstrLength);
                }
                if (isToReplace) // Do the replacement
                {
                    isToReplace = false;
                    if (replacementChars is null)
                    {
                        //first replacement target
                        //  replacementChars = ArrayPool<char>.Shared.Rent(size);

                        int size = ((length / oldstrLength) * (oldstrLength > newStrLength ? oldstrLength : newStrLength));
                        replacementChars = new char[size];
                        //copy first set of char that did not match.
                        //  Buffer.BlockCopy(_buffer, 0, replacementChars, 0, i * 2);
                        //   value.TryCopyTo(_buffer.AsSpan(_bufferPos, _charsCapacity - _bufferPos));
                        var buffer = FindBucketForIndex(0);
                        for (var a = 0; a < i; a++)
                        {
                            if (a >= buffer.OffsetLength)
                            {
                                buffer = FindBucketForIndex(buffer.OffsetLength);
                            }

                            replacementChars[a] = buffer.Buffer[a - buffer.Offset];
                        }


                        // new Span<char>(_buffer, 0, i).TryCopyTo(new Span<char>(replacementChars, 0, i));
                        index = i;
                    }

                    i += oldstrLength - 1;
                    for (int k = 0; k < newStrLength; k++)
                    {
                        replacementChars[index++] = newStr[k];
                    }
                }
                else if (replacementChars != null)// No replacement, copy the old character
                {
                    //could batch these up instead one at a time!
                    replacementChars[index++] = bucketChar;
                }
            }//end for

            if (replacementChars != null)
            {
                //if (_bucketIndex > 0)
                //{
                //    for (var i = 1; i < _bucketIndex; i++)
                //    {
                //        _buckets[i] = default;
                //    }
                //}


                // Copy back the new string into _chars
                // EnsureCapacity(index - _bufferPos);
                // new Span<char>(replacementChars, 0, index).TryCopyTo(new Span<char>(_buffer));
                if (_bucketIndex == 0)
                {
                    EnsureBucketCapacity();
                }

                _buckets[0] = new Bucket(replacementChars, index, 0);
                _totalOffset = index;
                _bucketIndex = 1;

                //ArrayPool<char>.Shared.Return(replacementChars);
                // _bufferPos = index;
                // _bucketIndex = 0;
                _bufferPos = 0;
            }


            return this;
        }

        public LiteStringBuilder13 Replace_TryCache2(string oldStr, string newStr)
        {
            int length = this.Length;


            if (length == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0)
                return this;

            if (newStr == null)
                newStr = "";

            int newStrLength = newStr.Length;

            int index = 0;
            char[] replacementChars = null;

            var bucket = FindBucketForIndex(0);
            bool isToReplace = false;

            unsafe
            {
                fixed (char* oldPtr = oldStr)
                {

                    // Create the new string into _replacement
                    for (int i = 0; i < length; i++)
                    {
                        if (i >= bucket.OffsetLength)
                        {
                            bucket = FindBucketForIndex(bucket.OffsetLength);
                        }

                        char bucketChar = bucket.Buffer[i - bucket.Offset];
                        if (bucketChar == oldPtr[0]) // If first character found, check for the rest of the string to replace
                        {
                            int k = 1;//skip one char

                            if (i + k >= bucket.OffsetLength)
                            {
                                bucket = FindBucketForIndex(bucket.OffsetLength);
                            }


                            while (k < oldstrLength && bucket.Buffer[i + k - bucket.Offset] == oldPtr[k])
                            {
                                k++;

                                if (i + k >= bucket.OffsetLength)
                                {
                                    bucket = FindBucketForIndex(bucket.OffsetLength);
                                }
                            }

                            isToReplace = (k == oldstrLength);
                        }
                        if (isToReplace) // Do the replacement
                        {
                            isToReplace = false;
                            if (replacementChars is null)
                            {
                                //first replacement target
                                //  replacementChars = ArrayPool<char>.Shared.Rent(size);

                                int size = ((length / oldstrLength) * (oldstrLength > newStrLength ? oldstrLength : newStrLength));
                                replacementChars = new char[size];
                                //copy first set of char that did not match.
                                //  Buffer.BlockCopy(_buffer, 0, replacementChars, 0, i * 2);
                                //   value.TryCopyTo(_buffer.AsSpan(_bufferPos, _charsCapacity - _bufferPos));
                                var buffer = FindBucketForIndex(0);
                                for (var a = 0; a < i; a++)
                                {
                                    if (a >= buffer.OffsetLength)
                                    {
                                        buffer = FindBucketForIndex(buffer.OffsetLength);
                                    }

                                    replacementChars[a] = buffer.Buffer[a - buffer.Offset];
                                }


                                // new Span<char>(_buffer, 0, i).TryCopyTo(new Span<char>(replacementChars, 0, i));
                                index = i;
                            }

                            i += oldstrLength - 1;
                            for (int k = 0; k < newStrLength; k++)
                            {
                                replacementChars[index++] = newStr[k];
                            }
                        }
                        else if (replacementChars != null)// No replacement, copy the old character
                        {
                            //could batch these up instead one at a time!
                            replacementChars[index++] = bucketChar;
                        }
                    }//end for


                }
            }

            if (replacementChars != null)
            {
                //if (_bucketIndex > 0)
                //{
                //    for (var i = 1; i < _bucketIndex; i++)
                //    {
                //        _buckets[i] = default;
                //    }
                //}


                // Copy back the new string into _chars
                // EnsureCapacity(index - _bufferPos);
                // new Span<char>(replacementChars, 0, index).TryCopyTo(new Span<char>(_buffer));
                if (_bucketIndex == 0)
                {
                    EnsureBucketCapacity();
                }

                _buckets[0] = new Bucket(replacementChars, index, 0);
                _totalOffset = index;
                _bucketIndex = 1;

                //ArrayPool<char>.Shared.Return(replacementChars);
                // _bufferPos = index;
                // _bucketIndex = 0;
                _bufferPos = 0;
            }


            return this;
        }

        public LiteStringBuilder13 Replace_TryCache3(string oldStr, string newStr)
        {
            int length = this.Length;


            if (length == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0)
                return this;

            if (newStr == null)
                newStr = "";

            int newStrLength = newStr.Length;

            int index = 0;
            char[] replacementChars = null;

            var bucket = FindBucketForIndex(0);
            bool isToReplace = false;

            unsafe
            {
                fixed (char* oldPtr = oldStr)
                {

                    // Create the new string into _replacement
                    for (int i = 0; i < length; i++)
                    {
                        if (i >= bucket.OffsetLength)
                        {
                            bucket = FindBucketForIndex(bucket.OffsetLength);
                        }

                        char bucketChar = bucket.Buffer[i - bucket.Offset];
                        if (bucketChar == oldPtr[0]) // If first character found, check for the rest of the string to replace
                        {
                            int k = 1;//skip one char

                            if (i + k >= bucket.OffsetLength)
                            {
                                bucket = FindBucketForIndex(bucket.OffsetLength);
                            }


                            while (k < oldstrLength && bucket.Buffer[i + k - bucket.Offset] == oldPtr[k])
                            {
                                k++;

                                if (i + k >= bucket.OffsetLength)
                                {
                                    bucket = FindBucketForIndex(bucket.OffsetLength);
                                }
                            }

                            isToReplace = (k == oldstrLength);
                        }
                        if (isToReplace) // Do the replacement
                        {
                            isToReplace = false;
                            if (replacementChars is null)
                            {
                                //first replacement target
                                //  replacementChars = ArrayPool<char>.Shared.Rent(size);

                                int size = ((length / oldstrLength) * (oldstrLength > newStrLength ? oldstrLength : newStrLength));
                                replacementChars = new char[size];
                                //copy first set of char that did not match.
                                //  Buffer.BlockCopy(_buffer, 0, replacementChars, 0, i * 2);
                                //   value.TryCopyTo(_buffer.AsSpan(_bufferPos, _charsCapacity - _bufferPos));
                                var buffer = FindBucketForIndex(0);

                                fixed (char* targetPtr = replacementChars)
                                {
                                    for (var a = 0; a < i; a++)
                                    {
                                        if (a >= buffer.OffsetLength)
                                        {
                                            buffer = FindBucketForIndex(buffer.OffsetLength);
                                        }

                                        targetPtr[a] = buffer.Buffer[a - buffer.Offset];
                                    }
                                }


                                // new Span<char>(_buffer, 0, i).TryCopyTo(new Span<char>(replacementChars, 0, i));
                                index = i;
                            }

                            i += oldstrLength - 1;

                            if (newStrLength > 0)
                            {
                                fixed (char* newPtr = newStr)
                                {
                                    for (int k = 0; k < newStrLength; k++)
                                    {
                                        replacementChars[index++] = newPtr[k];
                                    }
                                }
                            }
                        }
                        else if (replacementChars != null)// No replacement, copy the old character
                        {
                            //could batch these up instead one at a time!
                            replacementChars[index++] = bucketChar;
                        }
                    }//end for


                }
            }

            if (replacementChars != null)
            {
                //if (_bucketIndex > 0)
                //{
                //    for (var i = 1; i < _bucketIndex; i++)
                //    {
                //        _buckets[i] = default;
                //    }
                //}


                // Copy back the new string into _chars
                // EnsureCapacity(index - _bufferPos);
                // new Span<char>(replacementChars, 0, index).TryCopyTo(new Span<char>(_buffer));
                if (_bucketIndex == 0)
                {
                    EnsureBucketCapacity();
                }

                _buckets[0] = new Bucket(replacementChars, index, 0);
                _totalOffset = index;
                _bucketIndex = 1;

                //ArrayPool<char>.Shared.Return(replacementChars);
                // _bufferPos = index;
                // _bucketIndex = 0;
                _bufferPos = 0;
            }


            return this;
        }

        public LiteStringBuilder13 Replace_TryCacheRework(string oldStr, string newStr)
        {
            int length = this.Length;


            if (length == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0)
                return this;

            if (newStr == null)
                newStr = "";

            int newStrLength = newStr.Length;


            int index = 0;
            char[] replacementChars = null;
            int replaceIndex = 0;

            var bucket = FindBucketForIndex(0);
            int matchIndex = 0;
            // Create the new string into _replacement
            for (int i = 0; i < length; i++)
            {
                if (i >= bucket.OffsetLength)
                {
                    bucket = FindBucketForIndex(bucket.OffsetLength);
                }

                char bucketChar = bucket.Buffer[i - bucket.Offset];
                if (bucketChar == oldStr[matchIndex]) // If first character found, check for the rest of the string to replace
                {
                    matchIndex++;

                    if (matchIndex == oldstrLength) // Do the replacement
                    {
                        if (replaceIndex == 0)
                        {
                            //first replacement target
                            //  replacementChars = ArrayPool<char>.Shared.Rent(size);

                            int deltaLength = oldstrLength > newStrLength ? oldstrLength - newStrLength : newStrLength - oldstrLength;
                            int size = ((length / oldstrLength) * (oldstrLength + deltaLength)) + 1;
                            replacementChars = new char[size];
                            //copy first set of char that did not match.
                            //  Buffer.BlockCopy(_buffer, 0, replacementChars, 0, i * 2);
                            //   value.TryCopyTo(_buffer.AsSpan(_bufferPos, _charsCapacity - _bufferPos));
                            var buffer = FindBucketForIndex(0);
                            index = i - oldstrLength + 1;
                            for (var a = 0; a < index; a++)
                            {
                                if (a >= buffer.OffsetLength)
                                {
                                    buffer = FindBucketForIndex(buffer.OffsetLength);
                                }

                                if (index > buffer.OffsetLength)
                                {
                                    buffer.Span.TryCopyTo(new Span<char>(replacementChars, a, buffer.Length));
                                    a += (buffer.Length - 1);
                                }
                                else
                                {
                                    replacementChars[a] = buffer.Buffer[a - buffer.Offset];
                                }
                            }


                            // new Span<char>(_buffer, 0, i).TryCopyTo(new Span<char>(replacementChars, 0, i));
                            // index = i;
                        }
                        else
                        {
                            index -= oldstrLength - 1;
                        }

                        replaceIndex++;
                        //  i += oldstrLength - 1;
                        for (int k = 0; k < newStrLength; k++)
                        {
                            replacementChars[index++] = newStr[k];
                        }

                        matchIndex = 0;
                    }
                    else if (replaceIndex > 0)
                    {
                        replacementChars[index++] = bucketChar;
                    }
                }
                else if (replaceIndex > 0)// No replacement, copy the old character
                {
                    //could batch these up instead one at a time!
                    replacementChars[index++] = bucketChar;
                    matchIndex = 0;
                }
                else
                {
                    matchIndex = 0;
                }
            }//end for

            if (replaceIndex > 0)
            {
                //if (_bucketIndex > 0)
                //{
                //    for (var i = 1; i < _bucketIndex; i++)
                //    {
                //        _buckets[i] = default;
                //    }
                //}

                if (_bucketIndex == 0)
                {
                    EnsureBucketCapacity();
                }

                // Copy back the new string into _chars
                // EnsureCapacity(index - _bufferPos);
                // new Span<char>(replacementChars, 0, index).TryCopyTo(new Span<char>(_buffer));
                _buckets[0] = new Bucket(replacementChars, index, 0);
                _totalOffset = index;
                _bucketIndex = 1;

                //ArrayPool<char>.Shared.Return(replacementChars);
                // _bufferPos = index;
                // _bucketIndex = 0;
                _bufferPos = 0;
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

                    //if (pos != currentCapacity)
                    //{
                    //    //trim
                    //    char[] newItems = new char[pos];
                    //    Buffer.BlockCopy(buffer, 0, newItems, 0, pos * 2);
                    //    //new Span<char>(buffer, 0, pos).TryCopyTo(new Span<char>(newItems));
                    //    buffer = newItems;
                    //}

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
            if (len == index)
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
