using System;
using static Age.Polyfills.HexConverter;

namespace Age.Polyfills
{
    public static class ConvertExtended
    {
        public static string ToHexStringLower(byte[] buffer)
        {
            return HexConverter.ToString(buffer.AsSpan(), Casing.Lower);
        }

        public static bool TryFromBase64Chars(ReadOnlySpan<char> chars, Span<byte> bytes, out int bytesWritten)
        {
            try
            {
                byte[] converted = Convert.FromBase64CharArray(chars.ToArray(), 0, chars.Length);
                converted.CopyTo(bytes);
                bytesWritten = converted.Length;
                return true;
            }
            catch
            {
                bytesWritten = 0;
                return false;
            }
        }

        public static bool TryToBase64Chars(ReadOnlySpan<byte> bytes, Span<char> chars, out int charsWritten,
            Base64FormattingOptions options = System.Base64FormattingOptions.None)
        {
            try
            {
                char[] charsBuffer = new char[chars.Length];
                charsWritten = Convert.ToBase64CharArray(bytes.ToArray(), 0, bytes.Length, charsBuffer, 0, options);
                charsBuffer.CopyTo(chars);
                return true;
            }
            catch
            {
                charsWritten = 0;
                return false;
            }
        }
    }
}
