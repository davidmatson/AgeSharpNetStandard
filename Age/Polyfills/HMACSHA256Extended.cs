using System;
using System.Security.Cryptography;

namespace Age.Polyfills
{
    static class HMACSHA256Extended
    {
        public static void HashData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> source, Span<byte> destination)
        {
            using (var hasher = new HMACSHA256())
            {
                hasher.Key = key.ToArray();
                byte[] copy = hasher.ComputeHash(source.ToArray());
                copy.CopyTo(destination);
            }
        }
    }
}
