using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Age.Polyfills
{
    public static class RandomExtensions
    {
        public static void NextBytes(this Random random, Span<byte> buffer)
        {
            byte[] doubleBuffer = new byte[buffer.Length];
            random.NextBytes(doubleBuffer);
            doubleBuffer.CopyTo(buffer);
        }

        public static long NextInt64(this Random random, long minValue, long maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", "minValue is greater than maxValue.");
            }

            ulong exclusiveRange = (ulong)(maxValue - minValue);

            if (exclusiveRange > 1)
            {
                // Narrow down to the smallest range [0, 2^bits] that contains maxValue - minValue
                // Then repeatedly generate a value in that outer range until we get one within the inner range.
                int bits = Log2Ceiling(exclusiveRange);
                while (true)
                {
                    ulong result = NextUInt64(random) >> (sizeof(long) * 8 - bits);
                    if (result < exclusiveRange)
                    {
                        return (long)result + minValue;
                    }
                }
            }

            Debug.Assert(minValue == maxValue || minValue + 1 == maxValue);
            return minValue;
        }

        static int SoftwareFallback(ulong value)
        {
            const ulong c1 = 0x_55555555_55555555ul;
            const ulong c2 = 0x_33333333_33333333ul;
            const ulong c3 = 0x_0F0F0F0F_0F0F0F0Ful;
            const ulong c4 = 0x_01010101_01010101ul;

            value -= (value >> 1) & c1;
            value = (value & c2) + ((value >> 2) & c2);
            value = (((value + (value >> 4)) & c3) * c4) >> 56;

            return (int)value;
        }

        /// <summary>Returns the integer (ceiling) log of the specified value, base 2.</summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Log2Ceiling(ulong value)
        {
            int result = Log2(value);
            if (SoftwareFallback(value) != 1)
            {
                result++;
            }
            return result;
        }

        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2.
        /// Note that by convention, input value 0 returns 0 since log(0) is undefined.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int Log2(ulong value)
        {
            value |= 1;

            uint hi = (uint)(value >> 32);

            if (hi == 0)
            {
                return Log2((uint)value);
            }

            return 32 + Log2(hi);
        }
        
        /// <summary>Produces a value in the range [0, ulong.MaxValue].</summary>
        private static ulong NextUInt64(this Random random) =>
             ((ulong)(uint)random.Next(1 << 22)) |
            (((ulong)(uint)random.Next(1 << 22)) << 22) |
            (((ulong)(uint)random.Next(1 << 20)) << 44);
    }
}
