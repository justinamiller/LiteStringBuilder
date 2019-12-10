using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StringHelper
{
    internal static class Utilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetIntLength(ulong n)
        {
            if (n < 10L) return 1;
            if (n < 100L) return 2;
            if (n < 1000L) return 3;
            if (n < 10000L) return 4;
            if (n < 100000L) return 5;
            if (n < 1000000L) return 6;
            if (n < 10000000L) return 7;
            if (n < 100000000L) return 8;
            if (n < 1000000000L) return 9;
            if (n < 10000000000L) return 10;
            if (n < 100000000000L) return 11;
            if (n < 1000000000000L) return 12;
            if (n < 10000000000000L) return 13;
            if (n < 100000000000000L) return 14;
            if (n < 1000000000000000L) return 15;
            if (n < 10000000000000000L) return 16;
            if (n < 100000000000000000L) return 17;
            if (n < 1000000000000000000L) return 18;
            if (n < 10000000000000000000L) return 19;

            return 20;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetPaddedCapacity(int bufferSize)
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
            return 16 << (num2 + (int)num1);
        }
    }
}
