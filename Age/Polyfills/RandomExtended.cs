using System;

namespace Age.Polyfills
{
    public static class RandomExtended
    {
        public class RandomThreadSafe
        {
            readonly Random random = new Random();
            readonly object sync = new object();

            public void NextBytes(byte[] buffer)
            {
                lock (sync)
                {
                    random.NextBytes(buffer);
                }
            }
        }

        public static RandomThreadSafe Shared = new RandomThreadSafe();
    }
}
