using System.Collections.Generic;

namespace Age.Polyfills
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;

            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }

            return default(TValue);
        }
    }
}
