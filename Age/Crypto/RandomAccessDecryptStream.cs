using System;
using System.IO;
using Age.Polyfills;

namespace Age.Crypto
{
    internal sealed class RandomAccessDecryptStream : Stream
    {
        readonly AgeRandomAccess _reader;
        readonly long _initialOffset;

        public RandomAccessDecryptStream(AgeRandomAccess reader, long initialOffset)
        {
            _reader = reader;
            _initialOffset = initialOffset;
            _position = _initialOffset;
            _length = _reader.PlaintextLength;
        }

        private long _position;
        private readonly long _length;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;

        public override long Position
        {
            get => _position;
            set
            {
                ArgumentOutOfRangeExceptionExtended.ThrowIfNegative(value);
                _position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
            => Read(buffer.AsSpan(offset, count));

        public int Read(Span<byte> buffer)
        {
            if (_position >= _length)
                return 0;

            var read = _reader.ReadAt(_position, buffer);
            _position += read;

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPos;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPos = offset;
                    break;
                case SeekOrigin.Current:
                    newPos = _position + offset;
                    break;
                case SeekOrigin.End:
                    newPos = _length + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            }

            ArgumentOutOfRangeExceptionExtended.ThrowIfNegative(newPos, nameof(offset));

            _position = newPos;
            return _position;
        }

        public override void Flush()
        {
        }

        public override void SetLength(long value) =>
            throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }
}
