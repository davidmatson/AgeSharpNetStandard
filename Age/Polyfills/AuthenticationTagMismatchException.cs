using System.Security.Cryptography;

namespace Age.Polyfills
{
    public class AuthenticationTagMismatchException : CryptographicException
    {
        public AuthenticationTagMismatchException() : base("Authentication failed during decryption.")
        {
        }
    }
}
