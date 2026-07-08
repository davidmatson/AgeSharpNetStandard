using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using impl = Org.BouncyCastle.Crypto.Modes;

namespace Age.Polyfills
{
    internal class ChaCha20Poly1305 : IDisposable
    {
        impl.ChaCha20Poly1305 _cipher;
        readonly byte[] _key;

        public ChaCha20Poly1305(ReadOnlySpan<byte> key)
        {
            _cipher = new impl.ChaCha20Poly1305();
            _key = key.ToArray();
        }

        public void Decrypt(ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> ciphertext, ReadOnlySpan<byte> tag,
            Span<byte> plaintext, ReadOnlySpan<byte> associatedData = default)
        {
            // Combine ciphertext + tag for BouncyCastle
            byte[] input = new byte[ciphertext.Length + tag.Length];
            ciphertext.CopyTo(input);
            tag.CopyTo(input.AsSpan(ciphertext.Length));

            var parameters = new AeadParameters(new KeyParameter(_key), 128, nonce.ToArray(), associatedData.ToArray());

            byte[] plaintextBuffer = new byte[plaintext.Length];
            _cipher.Init(false, parameters); // false = decrypt
            int len = _cipher.ProcessBytes(input, 0, input.Length, plaintextBuffer, 0);

            try
            {
                _cipher.DoFinal(plaintextBuffer, len);
            }
            catch (InvalidCipherTextException)
            {
                throw new AuthenticationTagMismatchException();
            }

            plaintextBuffer.CopyTo(plaintext);
        }

        public void Encrypt(ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> plaintext, Span<byte> ciphertext,
            Span<byte> tag, ReadOnlySpan<byte> associatedData = default)
        {
            if (nonce == null || nonce.Length != 12)
                throw new ArgumentException("Nonce must be 12 bytes.");

            var parameters = new AeadParameters(new KeyParameter(_key), 128, nonce.ToArray(), associatedData.ToArray());

            _cipher.Init(true, parameters);
            byte[] ciphertextTagBuffer = new byte[ciphertext.Length + tag.Length];

            int len = _cipher.ProcessBytes(plaintext.ToArray(), 0, plaintext.Length, ciphertextTagBuffer, 0);
            _cipher.DoFinal(ciphertextTagBuffer, len);

            // Extract tag from the end of ciphertext
            ciphertextTagBuffer.AsSpan().Slice(0, plaintext.Length).CopyTo(ciphertext);
            ciphertextTagBuffer.AsSpan().Slice(plaintext.Length).CopyTo(tag);
        }

        public void Dispose()
        {
        }
    }
}
