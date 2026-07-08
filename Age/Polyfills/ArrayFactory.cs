namespace Age.Polyfills
{
    static class ArrayFactory
    {
        public static T[] Concatenate<T>(T[] left, T[] right)
        {
            T[] result = new T[left.Length + right.Length];
            left.CopyTo(result, 0);
            right.CopyTo(result, left.Length);
            return result;
        }
    }
}
