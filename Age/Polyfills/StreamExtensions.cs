using System;
using System.IO;

namespace Age.Polyfills
{
    public static class StreamExtensions
    {
        public static int Read(this Stream stream, Span<byte> buffer)
        {
            byte[] doubleBuffer = new byte[buffer.Length];
            int result = stream.Read(doubleBuffer, 0, doubleBuffer.Length);

            if (result != -1)
                doubleBuffer.CopyTo(buffer);

            return result;
        }

        public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
        {
            byte[] doubleBuffer = buffer.ToArray();
            stream.Write(doubleBuffer, 0, doubleBuffer.Length);
        }
    }
}
