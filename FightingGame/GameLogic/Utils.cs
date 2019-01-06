using System.Collections.Generic;

namespace FightingGame.GameLogic
{
    public static class Utils
    {
        public static void Add<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> multiValueDictionary, TKey newKey, TValue newValue)
        {
            if (multiValueDictionary.TryGetValue(newKey, out var list))
            {
                list.Add(newValue);
            }
            else
            {
                multiValueDictionary[newKey] = new HashSet<TValue> { newValue };
            }
        }

        public static void Remove<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> multiValueDictionary, TKey key, TValue value)
        {
            if (multiValueDictionary.TryGetValue(key, out var list))
            {
                if (list.Contains(value))
                {
                    list.Remove(value);
                }

                if (list.Count == 0)
                {
                    multiValueDictionary.Remove(key);
                }
            }
        }

        public static IReadOnlyCollection<TValue> Get<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> multiValueDictionary, TKey key)
        {
            if (multiValueDictionary.TryGetValue(key, out var list))
            {
                return list;
            }
            return new TValue[]{ };
        }
    }
}
