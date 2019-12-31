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
[assembly: InternalsVisibleTo("LiteStringBuilder8.Tests")]
namespace StringHelper
{
    ///<summary>
    /// Mutable String class, optimized for speed and memory allocations while retrieving the final result as a string.
    /// Similar use than StringBuilder, but avoid a lot of allocations done by StringBuilder (conversion of int and float to string, frequent capacity change, etc.)
    ///</summary>
    public class LiteStringBuilder8
    {
        ///<summary>Working mutable string</summary>
        private char[] _buffer = null;
        private int _bufferPos = 0;
        private const int DefaultCapacity = 16;
        private readonly static char[] _charNumbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private readonly static CultureInfo s_Culture = CultureInfo.CurrentCulture;
        private int _totalLength = 0;

        private  Bucket[] _buckets = Array.Empty<Bucket>();
        private int _bucketIndex = 0;
#pragma warning disable HAA0501 // Explicit new array type allocation
        private readonly static char[][] s_bool = new char[2][]
#pragma warning restore HAA0501 // Explicit new array type allocation
  {
            new char[]{ 'F','a','l','s','e'},
            new char[]{ 'T', 'r','u','e' }
  };

        private int TotalLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _totalLength + _bufferPos;
            }
        }

        /// <summary>
        /// gets the size of data in the buffer pool
        /// </summary>
        public int Length
        {
            get
            {
                return _bufferPos;
            }
        }

        /// <summary>
        /// Get a new instance of LiteStringBuilder8
        /// </summary>
        /// <param name="initialCapacity"></param>
        /// <returns></returns>
        public static LiteStringBuilder8 Create(int initialCapacity = DefaultCapacity)
        {
            return new LiteStringBuilder8(initialCapacity);
        }

        public LiteStringBuilder8(int initialCapacity = DefaultCapacity)
        {
            int capacity = initialCapacity > 0 ? initialCapacity : DefaultCapacity;
            _buffer = new char[capacity];//Pool_Instance.Rent(capacity);
        }

        public LiteStringBuilder8(string value)
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
            return _bufferPos == 0;
        }


        ///<summary>Return the string</summary>
        public override string ToString()
        {
            int totalLength = TotalLength;
            if (totalLength == 0)
            {
                return string.Empty;
            }

            int pos = _bufferPos;
            int copyIndex = 0;

            int bucketLength = _bucketIndex;
            var buckets = _buckets;
            string allocString = new string('\0', totalLength);
            unsafe
            {
                fixed (char* destPtr = allocString)
                {

                    for (var i=0; i < bucketLength; i++)
                    {
                        var buffer = buckets[i];
                        fixed (char* sourcePtr = &buffer.Buffer[0])
                        {
                            int bufferLen = buffer.Length;
                            Buffer.MemoryCopy(sourcePtr, destPtr+ copyIndex, bufferLen * 2, bufferLen * 2);
                            copyIndex += bufferLen;
                        }
                    }

                    if (pos > 0)
                    {
                        fixed (char* sourcePtr = &_buffer[0])
                        {
                            Buffer.MemoryCopy(sourcePtr, destPtr + copyIndex, pos * 2, pos * 2);
                        }
                    }
                }
            }
            return allocString;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LiteStringBuilder8);
        }
        public bool Equals(LiteStringBuilder8 other)
        {
            // Check for null
            if (other is null)
                return false;

            // Check for same reference
            if (ReferenceEquals(this, other))
                return true;

            // Check for same Id and same Values
            if (other.Length != this.Length)
            {
                return false;
            }

            for (var i = 0; i < _bufferPos; i++)
            {
                if (!this._buffer[i].Equals(other._buffer[i]))
                {
                    return false;
                }
            }

            return true;

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
        public LiteStringBuilder8 Clear()
        {
            _totalLength = 0;
            _buckets = Array.Empty<Bucket>();
            _bufferPos = 0;
            return this;
        }

        ///<summary>Will allocate on the array creatation, and on boxing values</summary>
        public LiteStringBuilder8 Append(params object[] values)
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
        public LiteStringBuilder8 Append<T>(params T[] values)
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
        public LiteStringBuilder8 Append(string value)
        {
            if (value != null)
            {
                int n = value.Length;
                if (n > 0)
                {

                    EnsureCapacity(n);
                    int bytesSize = n * 2;
                    int pos = _bufferPos;
                    unsafe
                    {
                        fixed (char* valuePtr = value)
                        fixed (char* destPtr = &_buffer[pos])
                        {
                           Buffer.MemoryCopy(valuePtr, destPtr, bytesSize, bytesSize);
                        }
                    }
                    _bufferPos = n + pos;

                    //InternalAppend(value.AsSpan(), n);
                }
            }
            return this;
        }

        ///<summary>Generate a new line</summary>
        public LiteStringBuilder8 AppendLine()
        {
            return Append(Environment.NewLine);
        }

        ///<summary>Append a string and new line without memory allocation</summary>
        public LiteStringBuilder8 AppendLine(string value)
        {
            Append(value);
            return Append(Environment.NewLine);
        }

        ///<summary>Append a char without memory allocation</summary>
        public LiteStringBuilder8 Append(char value)
        {
            if (_bufferPos >= this._buffer.Length)
            {
                EnsureCapacity(1);
            }


            _buffer[_bufferPos++] = value;
            return this;
        }




        ///<summary>Append a bool without memory allocation</summary>
        public LiteStringBuilder8 Append(bool value)
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
        public LiteStringBuilder8 Append(char[] value)
        {
            if (value != null)
            {
                int n = value.Length;
                if (n > 0)
                {
                    //EnsureCapacity(n);
                    ////Buffer.BlockCopy(value, 0, _buffer, _bufferPos * 2, n * 2);

                    //_bufferPos += n;

                    EnsureCapacity(n);
                    new Span<char>(value).TryCopyTo(new Span<char>(_buffer, _bufferPos, n));
                    _bufferPos += n;
                }
            }


            return this;
        }


        ///<summary>Append an object.ToString(), allocate some memory</summary>
        public LiteStringBuilder8 Append(object value)
        {
            if (value is null)
                return this;

            return Append(value.ToString());
        }

        ///<summary>Append an datetime with small memory allocation</summary>
        public LiteStringBuilder8 Append(DateTime value)
        {
            return Append(value.ToString(s_Culture));
        }


        ///<summary>Append an sbyte with some memory allocation</summary>
        public LiteStringBuilder8 Append(sbyte value)
        {
            if (value < 0)
            {
                return Append((ulong)-((int)value), true);
            }
            return Append((ulong)value, false);
        }


        ///<summary>Append an byte with some memory allocation</summary>
        public LiteStringBuilder8 Append(byte value)
        {
            return Append(value, false);
        }



        ///<summary>Append an uint without memory allocation</summary>
        public LiteStringBuilder8 Append(uint value)
        {
            return Append((ulong)value, false);
        }

        ///<summary>Append an ulong without memory allocation</summary>
        public LiteStringBuilder8 Append(ulong value)
        {
            return Append(value, false);
        }

        ///<summary>Append an int without memory allocation</summary>
        public LiteStringBuilder8 Append(short value)
        {
            return Append((int)value);
        }


        ///<summary>Append an int without memory allocation</summary>
        public LiteStringBuilder8 Append(int value)
        {
            bool isNegative = value < 0;
            if (isNegative)
            {
                value = -value;
            }
            return Append((ulong)value, isNegative);
        }

        public LiteStringBuilder8 Append(float value)
        {
            return Append(value.ToString(s_Culture));
        }

        public LiteStringBuilder8 Append(decimal value)
        {
            return Append(value.ToString(s_Culture));
        }


        public LiteStringBuilder8 Append(long value)
        {
            bool isNegative = value < 0;
            if (isNegative)
            {
                value = -value;
            }
            return Append((ulong)value, isNegative);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LiteStringBuilder8 Append(ulong value, bool isNegative)
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
        public LiteStringBuilder8 Append(double value)
        {
            return Append(value.ToString(s_Culture));
        }

#region OLDCODE
#if !NETSTANDARD1_6 && !NETCOREAPP1_1
        [ExcludeFromCodeCoverage]
#endif
        private LiteStringBuilder8 InternalAppend(float valueF)
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
        private LiteStringBuilder8 InternalAppend(double value)
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

        ///<summary>Replace all occurences of a string by another one</summary>
        public LiteStringBuilder8 Replace(string oldStr, string newStr)
        {
            if (_bufferPos == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0)
                return this;

            if (newStr == null)
                newStr = "";

            int newStrLength = newStr.Length;

            int deltaLength = oldstrLength > newStrLength ? oldstrLength - newStrLength : newStrLength - oldstrLength;
            int size = ((this.TotalLength / oldstrLength) * (oldstrLength + deltaLength)) + 1;
            int index = 0;
            char[] replacementChars = null;
            int replaceIndex = 0;
            char firstChar = oldStr[0];
            // Create the new string into _replacement
            for (int i = 0; i < this.TotalLength; i++)
            {
                bool isToReplace = false;
                if (_buffer[i] == firstChar) // If first character found, check for the rest of the string to replace
                {
                    int k = 1;//skip one char
                    while (k < oldstrLength && _buffer[i + k] == oldStr[k])
                    {
                        k++;
                    }
                    isToReplace = (k == oldstrLength);
                }
                if (isToReplace) // Do the replacement
                {
                    if (replaceIndex == 0)
                    {
                        //first replacement target
                        replacementChars = new char[size];
                        //copy first set of char that did not match.
                        //  Buffer.BlockCopy(_buffer, 0, replacementChars, 0, i * 2);
                        //   value.TryCopyTo(_buffer.AsSpan(_bufferPos, _charsCapacity - _bufferPos));
                        new Span<char>(_buffer, 0, i).TryCopyTo(new Span<char>(replacementChars, 0, i));
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
                    replacementChars[index++] = _buffer[i];
                }
            }//end for

            if (replaceIndex > 0)
            {
                // Copy back the new string into _chars
                EnsureCapacity(index - _bufferPos);
                //   Buffer.BlockCopy(replacementChars, 0, _buffer, 0, index * 2);

                new Span<char>(replacementChars, 0, index).TryCopyTo(new Span<char>(_buffer));


                _bufferPos = index;
            }


            return this;
        }

        public LiteStringBuilder8 Replace2(string oldStr, string newStr)
        {
            if (_bufferPos == 0)
                return this;

            int oldstrLength = oldStr?.Length ?? 0;
            if (oldstrLength == 0)
                return this;

            if (newStr == null)
                newStr = "";

            int newStrLength = newStr.Length;

            int deltaLength = oldstrLength > newStrLength ? oldstrLength - newStrLength : newStrLength - oldstrLength;
            int size = ((_bufferPos / oldstrLength) * (oldstrLength + deltaLength)) + 1;
            int index = 0;
            char[] replacementChars = null;
            int replaceIndex = 0;
            char firstChar = oldStr[0];
            // Create the new string into _replacement
            for (int i = 0; i < _bufferPos; i++)
            {
                bool isToReplace = false;
                if (_buffer[i] == firstChar) // If first character found, check for the rest of the string to replace
                {
                    int k = 1;//skip one char
                    while (k < oldstrLength && _buffer[i + k] == oldStr[k])
                    {
                        k++;
                    }
                    isToReplace = (k == oldstrLength);
                }
                if (isToReplace) // Do the replacement
                {
                    if (replaceIndex == 0)
                    {
                        //first replacement target
                        replacementChars = ArrayPool<char>.Shared.Rent(size);
                        //copy first set of char that did not match.
                        //  Buffer.BlockCopy(_buffer, 0, replacementChars, 0, i * 2);
                        //   value.TryCopyTo(_buffer.AsSpan(_bufferPos, _charsCapacity - _bufferPos));
                        new Span<char>(_buffer, 0, i).TryCopyTo(new Span<char>(replacementChars, 0, i));
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
                    replacementChars[index++] = _buffer[i];
                }
            }//end for

            if (replaceIndex > 0)
            {
                // Copy back the new string into _chars
                EnsureCapacity(index - _bufferPos);
                //   Buffer.BlockCopy(replacementChars, 0, _buffer, 0, index * 2);

                new Span<char>(replacementChars, 0, index).TryCopyTo(new Span<char>(_buffer));

                ArrayPool<char>.Shared.Return(replacementChars);
                _bufferPos = index;
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
                int totalLength = this.TotalLength;
                int minCalc = (totalLength <= 8000) ? totalLength : 8000;
                int newCapacity = (appendLength >= minCalc) ? appendLength : minCalc; 
                char[] newBuffer = new char[newCapacity];
                if (pos > 0)
                {
                  //set size
                   EnsureBucketCapacity();
                
                    if (pos != currentCapacity)
                    {
                        char[] newItems = new char[pos];
                         Buffer.BlockCopy(newItems, 0, buffer, pos * 2, pos * 2);
                        buffer = newItems;
                    }
                    //copy data
                    _buckets[_bucketIndex++] = new Bucket(buffer, pos, _totalLength);
                    _totalLength += pos;
                    _bufferPos = 0;
                }

                _buffer = newBuffer;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureBucketCapacity()
        {
            int len = _buckets.Length;
            int index = _bucketIndex;
            if (len== index)
            {
                int newCapacity = len + 4;

                Bucket[] newItems = new Bucket[newCapacity];
                if (index > 0)
                {
                    Array.Copy(_buckets, 0, newItems, 0, index);
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
                    hash += _buffer[i].GetHashCode();
                }
                return 31 * hash + _bufferPos;
            }
        }
    }
}
