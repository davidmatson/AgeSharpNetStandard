using System;

namespace Age.Polyfills
{
    static class SpanExtensions
    {
        public static Span<char> TrimEnd(this Span<char> span, char value)
        {
            Span<char> result = span;

            while (!result.IsEmpty && result[result.Length - 1] == value)
            {
                result = result.Slice(0, result.Length - 1);
            }

            return result;
        }
    }
}
