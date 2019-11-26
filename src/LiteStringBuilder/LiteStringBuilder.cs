using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System;
using System.Buffers;
using System.Runtime.InteropServices;
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
            _buffer = ArrayPool<char>.Shared.Rent(_charsCapacity);
        }

        public LiteStringBuilder(string value)
        {
            if (value != null)
            {
                _charsCapacity = value.Length;
                _buffer = ArrayPool<char>.Shared.Rent(_charsCapacity);
                this.Append(value);
            }
            else
            {
                _charsCapacity = 32;
                _buffer = ArrayPool<char>.Shared.Rent(_charsCapacity);
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

                int bytesSize = n * 2;
                unsafe
                {
                    fixed (char* valuePtr = value)
                    fixed (char* destPtr = &_buffer[_bufferPos])
                    {

                        System.Buffer.MemoryCopy(valuePtr, destPtr, bytesSize, bytesSize);
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
            EnsureCapacity(1);
            _buffer[_bufferPos++] = value;
            return this;
        }


        ///<summary>Append a bool without memory allocation</summary>
        public LiteStringBuilder Append(bool value)
        {
            if (value)
            {
                return Append("True");
            }
            else
            {
                return Append("False");
            }
        }

        ///<summary>Append a char[] without memory allocation</summary>
        public LiteStringBuilder Append(char[] value)
        {
            int n = value.Length;
            EnsureCapacity(n);
            System.Buffer.BlockCopy(value, 0, _buffer, _bufferPos * 2, n * 2);
            _bufferPos += n;
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
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }


        ///<summary>Append an sbyte with some memory allocation</summary>
        public LiteStringBuilder Append(sbyte value)
        {
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }


        ///<summary>Append an byte with some memory allocation</summary>
        public LiteStringBuilder Append(byte value)
        {
            return Append(value.ToString(CultureInfo.CurrentCulture));
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
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        public LiteStringBuilder Append(decimal value)
        {
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }

        public LiteStringBuilder Append(short value)
        {
            if (value < 0)
            {
                int val = (int)value;
                val = -val;
                return Append((ulong)val, true);
            }

            return Append((ulong)value, false);
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
        private static int GetIntLength(ulong n)
        {
            int num = 0;
            do
            {
                num++;
                n /= 10uL;
            }
            while (n != 0uL);
            return num;
        }

        private LiteStringBuilder Append(ulong value, bool isNegative)
        {
            // Allocate enough memory to handle any ulong number
            int length = GetIntLength(value);

            if (isNegative)
            {
                length++;
            }
            EnsureCapacity(length);

            // Handle the negative case
            if (isNegative)
            {
                _buffer[_bufferPos++] = '-';
            }

            if (value >= 0 && value <= 9)
            {
                //between 0-9
                _buffer[_bufferPos++] = _charNumbers[value];
                return this;
            }

            // Copy the digits in reverse order
            int nbChars = 0;
            do
            {
                _buffer[_bufferPos++] = _charNumbers[value % 10];
                value /= 10;
                nbChars++;
            } while (value != 0);

            // Reverse the result
            for (int i = nbChars / 2 - 1; i >= 0; i--)
            {
                int aPtr = _bufferPos - i - 1;
                int bPtr = _bufferPos - nbChars + i;

                char c = _buffer[aPtr];
                _buffer[aPtr] = _buffer[bPtr];
                _buffer[bPtr] = c;
            }


            return this;
        }

        ///<summary>Append a double without memory allocation.</summary>
        public LiteStringBuilder Append(double value)
        {
            return Append(value.ToString(CultureInfo.CurrentCulture));
        }
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
        private LiteStringBuilder InternalAppend(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                Append(value.ToString(CultureInfo.CurrentCulture));
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

            // var replacement = new char[((_bufferPos / oldstrLength) * (oldstrLength + Math.Abs(oldstrLength - newStrLength)))+1];
            int deltaLength = oldstrLength > newStrLength ? oldstrLength - newStrLength : newStrLength - oldstrLength;
            int size = ((_bufferPos / oldstrLength) * (oldstrLength + deltaLength)) + 1;
            int index = 0;
            var pool = ArrayPool<char>.Shared;
            var replacementChars = pool.Rent(size);

            // Create the new string into _replacement
            for (int i = 0; i < _bufferPos; i++)
            {
                bool isToReplace = false;
                if (_buffer[i] == oldStr[0]) // If first character found, check for the rest of the string to replace
                {
                    int k = 1;
                    while (k < oldstrLength && _buffer[i + k] == oldStr[k])
                        k++;
                    isToReplace = (k >= oldstrLength);
                }
                if (isToReplace) // Do the replacement
                {
                    i += oldstrLength - 1;
                    if (newStr != null)
                        for (int k = 0; k < newStrLength; k++)
                            replacementChars[index++] = newStr[k];
                }
                else // No replacement, copy the old character
                    replacementChars[index++] = _buffer[i];
            }

            // Copy back the new string into _chars
            EnsureCapacity(index - _bufferPos);

            System.Buffer.BlockCopy(replacementChars, 0, _buffer, 0, index * 2);
            pool.Return(replacementChars);

            _bufferPos = index;
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int appendLength)
        {
            if (_bufferPos + appendLength > _charsCapacity)
            {
                //fix allocation
                // _charsCapacity= _bufferPos + appendLength;
                if (appendLength > _charsCapacity)
                {
                    //more than double size
                    _charsCapacity += appendLength;
                }
                else
                {
                    //increase size by double
                    _charsCapacity *= 2;
                }
                var pool = ArrayPool<char>.Shared;
                char[] newChars = pool.Rent(_charsCapacity);
                Buffer.BlockCopy(_buffer, 0, newChars, 0, _bufferPos * 2);
                _buffer = newChars;
                pool.Return(newChars);
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
