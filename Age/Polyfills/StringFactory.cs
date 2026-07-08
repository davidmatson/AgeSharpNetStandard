using System;
using System.Text;

namespace Age.Polyfills
{
    static class StringFactory
    {
        public static string Concatenate(ReadOnlySpan<char> value1, ReadOnlySpan<char> value2, ReadOnlySpan<char> value3)
        {
            int capacity = value1.Length + value2.Length + value3.Length;

            StringBuilder builder = new StringBuilder(capacity);

            foreach (char c in value1)
                builder.Append(c);
            foreach (char c in value2)
                builder.Append(c);
            foreach (char c in value3)
                builder.Append(c);

            return builder.ToString();
        }
    }
}
