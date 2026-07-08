using System;
using System.Linq;

namespace Age.Polyfills
{
    static class ReadOnlySpanExtensions
    {
        public static bool Contains(this ReadOnlySpan<char> span, char value)
        {
            foreach (char item in span)
                if (item == value)
                    return true;

            return false;
        }

        public static int IndexOfAnyExcept(this ReadOnlySpan<char> span, char[] values)
        {
            if (span.IsEmpty)
                return -1;

            for (int index = 0; index < span.Length; ++index)
                if (!values.Contains(span[index]))
                    return index;

            return -1;
        }
    }
}
