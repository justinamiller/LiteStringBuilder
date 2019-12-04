using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace StringHelper
{
    ///<summary>
    /// Mutable String class, optimized for speed and memory allocations while retrieving the final result as a string.
    /// Similar use than StringBuilder, but avoid a lot of allocations done by StringBuilder (conversion of int and float to string, frequent capacity change, etc.)
    ///</summary>
    public class LiteStringBuilder
    {
        ///<summary>Working mutable string</summary>
        private char[] _buffer = null;
        private int _bufferPos = 0;
        private int _charsCapacity = 0;
        private readonly static char[] _charNumbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private readonly static CultureInfo s_Culture = CultureInfo.CurrentCulture;
        private readonly static SimpleArrayPool<char> s_PoolInstance = new SimpleArrayPool<char>();


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
            get
            {
                return _bufferPos;
            }
        }

        /// <summary>
        /// Get a new instance of LiteStringBuilder
        /// </summary>
        /// <param name="initialCapacity"></param>
        /// <returns></returns>
        public static LiteStringBuilder Create(int initialCapacity = 32)
        {
            return new LiteStringBuilder(initialCapacity);
        }

        public LiteStringBuilder(int initialCapacity = 32)
        {
            _charsCapacity = initialCapacity > 0 ? initialCapacity : 32;
            _buffer = s_PoolInstance.Rent(_charsCapacity);
        }

        ///// <summary>
        ///// return buffer back to the pool
        ///// </summary>
        //~LiteStringBuilder()
        //{
        //    ArrayPool<char>.Shared.Return(_buffer);
        //}

        public LiteStringBuilder(string value)
        {
            if (value != null)
            {
                _charsCapacity = value.Length;
                _buffer = s_PoolInstance.Rent(_charsCapacity);
                this.Append(value);
            }
            else
            {
                _charsCapacity = 32;
                _buffer = s_PoolInstance.Rent(_charsCapacity);
            }
        }

        public bool IsEmpty()
        {
            return _bufferPos == 0;
        }

        ///<summary>Return the string</summary>
        public override string ToString()
        {
            if (_bufferPos == 0)
            {
                return string.Empty;
            }

            return new string(_buffer, 0, _bufferPos);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LiteStringBuilder);
        }
        public bool Equals(LiteStringBuilder other)
        {
            // Check for null
            if (ReferenceEquals(other, null))
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

        ///<summary>Will allocate a little memory due to boxing</summary>
        public void Set(params object[] str)
        {
            Clear();

            for (int i = 0; i < str.Length; i++)
            {
                Append(str[i]);
            }
        }


        ///<summary>Reset the string to empty</summary>
        public LiteStringBuilder Clear()
        {
            _bufferPos = 0;
            return this;
        }

        ///<summary>Append a string without memory allocation</summary>
        public LiteStringBuilder Append(string value)
        {
            int n = value?.Length ?? 0;
            if (n > 0)
            {
                EnsureCapacity(n);
                if (n <= 3)
                {
                    _buffer[_bufferPos] = value[0];
                    if (n > 2)
                    {
                        _buffer[_bufferPos + 1] = value[1];
                        _buffer[_bufferPos + 2] = value[2];
                    }
                    else if (n>1)
                    {
                        _buffer[_bufferPos + 1] = value[1];
                    }
                }
                else
                {
                    value.AsSpan().TryCopyTo(_buffer.AsSpan(_bufferPos, _charsCapacity - _bufferPos));
                }

                _bufferPos += n;
            }
            return this;
        }

        ///<summary>Append a string without memory allocation</summary>
        private LiteStringBuilder InternalAppendSafe(string value)
        {
            int n = value?.Length ?? 0;
            if (n > 0)
            {
                EnsureCapacity(n);
                int bytesSize = n * 2;
                unsafe
                {
                    fixed (char* valuePtr = value)
                    fixed (char* destPtr = &_buffer[_bufferPos])
                    {
                        System.Buffer.MemoryCopy(valuePtr, destPtr, bytesSize, bytesSize);
                        //System.Buffer.MemoryCopy((byte*)valuePtr, (byte*)destPtr, bytesSize, bytesSize);
                    }
                }

                _bufferPos += n;
            }

            return this;
        }


        ///<summary>Generate a new line</summary>
        public LiteStringBuilder AppendLine()
        {
            return Append(Environment.NewLine);
        }

        ///<summary>Append a string and new line without memory allocation</summary>
        public LiteStringBuilder AppendLine(string value)
        {
            Append(value);
            return Append(Environment.NewLine);
        }

        ///<summary>Append a char without memory allocation</summary>
        public LiteStringBuilder Append(char value)
        {
            if (_bufferPos >= _charsCapacity)
                EnsureCapacity(1);

            _buffer[_bufferPos++] = value;
            return this;
        }




        ///<summary>Append a bool without memory allocation</summary>
        public LiteStringBuilder Append(bool value)
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
        public LiteStringBuilder Append(char[] value)
        {
            int n = value?.Length ?? 0;
            if (n > 0)
            {
                EnsureCapacity(n);
                Buffer.BlockCopy(value, 0, _buffer, _bufferPos * 2, n * 2);
                _bufferPos += n;
            }

            return this;
        }

        ///<summary>Append an object.ToString(), allocate some memory</summary>
        public LiteStringBuilder Append(object value)
        {
            if (value is null)
                return this;

            return Append(value.ToString());
        }

        ///<summary>Append an datetime with small memory allocation</summary>
        public LiteStringBuilder Append(DateTime value)
        {
            return InternalAppendSafe(value.ToString(s_Culture));
        }


        ///<summary>Append an sbyte with some memory allocation</summary>
        public LiteStringBuilder Append(sbyte value)
        {
            return InternalAppendSafe(value.ToString(s_Culture));
        }


        ///<summary>Append an byte with some memory allocation</summary>
        public LiteStringBuilder Append(byte value)
        {
            return InternalAppendSafe(value.ToString(s_Culture));
        }



        ///<summary>Append an uint without memory allocation</summary>
        public LiteStringBuilder Append(uint value)
        {
            return Append((ulong)value, false);
        }

        ///<summary>Append an ulong without memory allocation</summary>
        public LiteStringBuilder Append(ulong value)
        {
            return Append(value, false);
        }

        ///<summary>Append an int without memory allocation</summary>
        public LiteStringBuilder Append(int value)
        {
            bool isNegative = value < 0;
            if (isNegative)
            {
                value = -value;
            }
            return Append((ulong)value, isNegative);
        }

        public LiteStringBuilder Append(float value)
        {
            return InternalAppendSafe(value.ToString(s_Culture));
        }

        public LiteStringBuilder Append(decimal value)
        {
            return InternalAppendSafe(value.ToString(s_Culture));
        }


        public LiteStringBuilder Append(long value)
        {
            bool isNegative = value < 0;
            if (isNegative)
            {
                value = -value;
            }
            return Append((ulong)value, isNegative);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LiteStringBuilder Append(ulong value, bool isNegative)
        {
            // Allocate enough memory to handle any ulong number
            int length = Utilities.GetIntLength(value);


            EnsureCapacity(length + (isNegative ? 1:0));
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
            int nbChars = _bufferPos-1;
            do
            {
                buffer[nbChars--] = _charNumbers[value % 10];
                value /= 10;
            } while (value != 0);

            return this;
        }

        ///<summary>Append a double without memory allocation.</summary>
        public LiteStringBuilder Append(double value)
        {
            return InternalAppendSafe(value.ToString(s_Culture));
        }

#if !NETSTANDARD1_3
        [ExcludeFromCodeCoverage]
#endif
        private LiteStringBuilder InternalAppend(float valueF)
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

#if !NETSTANDARD1_3
        [ExcludeFromCodeCoverage]
#endif
        private LiteStringBuilder InternalAppend(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                InternalAppendSafe(value.ToString(s_Culture));
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
                if (_bufferPos + nbChars + 2 >= _charsCapacity)
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

        ///<summary>Replace all occurences of a string by another one</summary>
        public LiteStringBuilder Replace(string oldStr, string newStr)
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
                        k++;
                    isToReplace = (k >= oldstrLength);
                }
                if (isToReplace) // Do the replacement
                {
                    if (replaceIndex == 0)
                    {
                        replacementChars = s_PoolInstance.Rent(size);
                        //copy first set of char that did not match.
                        Buffer.BlockCopy(_buffer, 0, replacementChars, 0, i * 2);
                       // Array.Copy(_buffer, 0, replacementChars, 0, i);
                        index = i;
                    }

                    replaceIndex++;
                    i += oldstrLength - 1;
                    if (newStr != null)
                        for (int k = 0; k < newStrLength; k++)
                            replacementChars[index++] = newStr[k];
                }
                else if (replaceIndex > 0)// No replacement, copy the old character
                    replacementChars[index++] = _buffer[i];
            }//end for

            if (replaceIndex > 0)
            {
                // Copy back the new string into _chars
                EnsureCapacity(index - _bufferPos);
                Buffer.BlockCopy(replacementChars, 0, _buffer, 0, index * 2);
                s_PoolInstance.Return(replacementChars);
                _bufferPos = index;
            }


            return this;
        }


         [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int appendLength)
        {
            int capacity = _charsCapacity;
            int pos = _bufferPos;
            if (pos + appendLength > capacity)
            {
                // int newSize = (int)((appendLength + capacity) * 1.5);
               int  newSize = capacity + appendLength;
                if (250 > newSize)
                {
                    capacity = 250;
                }
                else
                {
                    capacity = newSize;
                }
                //fix allocation
                // capacity = _bufferPos + appendLength;

                //if (appendLength > _charsCapacity)
                //{
                //    //more than double size
                //    _charsCapacity += appendLength;
                //}
                //else
                //{
                //    //increase size by double
                //    _charsCapacity *= 2;
                //}

                char[] newBuffer = s_PoolInstance.Rent(capacity);
                // char[] newBuffer = ArrayPool<char>.Shared.Rent(capacity);
                //char[] newBuffer = new char[capacity];
                if (pos > 0)
                {
                    //copy data over as bytes
                    Buffer.BlockCopy(_buffer, 0, newBuffer, 0, pos * 2);
                    //binary copy can return.
                    s_PoolInstance.Return(_buffer);
                }

                _buffer = null;
                _buffer = newBuffer;
                _charsCapacity = capacity;
                //      pool.Return(newBuffer);
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
