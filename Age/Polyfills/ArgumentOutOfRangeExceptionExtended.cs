using System;

namespace Age.Polyfills
{
    static class ArgumentOutOfRangeExceptionExtended
    {
        public static void ThrowIfNegative(long value, string paramName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName);
            }
        }

        public static void ThrowIfNegativeOrZero(long value, string paramName = null)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName);
            }
        }
    }
}
