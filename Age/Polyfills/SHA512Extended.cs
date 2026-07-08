using System.Security.Cryptography;

namespace Age.Polyfills
{
    public static class SHA512Extended
    {
        public static byte[] HashData(byte[] buffer)
        {
            using (var hasher = SHA512.Create())
            {
                return hasher.ComputeHash(buffer);
            }
        }
    }
}
