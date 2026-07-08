using System.Security.Cryptography;

namespace Age.Polyfills
{
    public static class SHA256Extended
    {
        public static byte[] HashData(byte[] buffer)
        {
            using (var hasher = SHA256.Create())
            {
                return hasher.ComputeHash(buffer);
            }
        }
    }
}
