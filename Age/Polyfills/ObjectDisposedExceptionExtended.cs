using System;

namespace Age.Polyfills
{
    static class ObjectDisposedExceptionExtended
    {
        public static void ThrowIf(bool condition, object instance)
        {
            if (condition)
            {
                throw new ObjectDisposedException(instance != null ? instance.GetType().Name : null);
            }
        }
    }
}
