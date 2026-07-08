using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Age.Polyfills
{
    public static class RandomNumberGeneratorExtended
    {
        public static void Fill(byte[] buffer)
        {
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(buffer);
            }
        }

        public static void Fill(Span<byte> buffer)
        {
            byte[] doubleBuffer = new byte[buffer.Length];
            Fill(doubleBuffer);
            doubleBuffer.CopyTo(buffer);
        }

        public static int GetInt32(int toExclusive)
        {
            ArgumentOutOfRangeExceptionExtended.ThrowIfNegativeOrZero(toExclusive);

            return GetInt32(0, toExclusive);
        }

        public static int GetInt32(int fromInclusive, int toExclusive)
        {
            if (fromInclusive >= toExclusive)
                throw new ArgumentException("Invalid random range.");

            // The total possible range is [0, 4,294,967,295).
            // Subtract one to account for zero being an actual possibility.
            uint range = (uint)toExclusive - (uint)fromInclusive - 1;

            // If there is only one possible choice, nothing random will actually happen, so return
            // the only possibility.
            if (range == 0)
            {
                return fromInclusive;
            }

            // Create a mask for the bits that we care about for the range. The other bits will be
            // masked away.
            uint mask = range;
            mask |= mask >> 1;
            mask |= mask >> 2;
            mask |= mask >> 4;
            mask |= mask >> 8;
            mask |= mask >> 16;

            uint[] oneUint = new uint[] { 0 };
            Span<byte> oneUintBytes = MemoryMarshal.AsBytes(oneUint.AsSpan());

            uint result;

            do
            {
                Fill(oneUintBytes);
                result = mask & oneUint[0];
            }
            while (result > range);

            return (int)result + fromInclusive;
        }
    }
}
