namespace Age.Polyfills
{
    static class StringExtensions
    {
        public static int IndexOfAnyExceptInRange(this string value, char start, char end)
        {
            if (value.Length == 0)
                return -1;

            for (int index = 0; index < value.Length; ++index)
                if (value[index] < start || value[index] > end)
                    return index;

            return -1;
        }
    }
}
