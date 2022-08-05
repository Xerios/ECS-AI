using System.Collections.Generic;
using System.Linq;

public static class MultimapExt
{
    public static bool Add<TKey, TValue, TCollection>(this IDictionary<TKey, TCollection> dictionary, TKey key, TValue value) where TCollection : HashSet<TValue>, new()
    {
        TCollection collection;

        if (!dictionary.TryGetValue(key, out collection)) {
            collection = new TCollection();
            dictionary.Add(key, collection);
        }

        return collection.Add(value);
    }

    public static bool Remove<TKey, TValue, TCollection>(this IDictionary<TKey, TCollection> dictionary, TKey key, TValue value) where TCollection : HashSet<TValue>
    {
        TCollection collection;

        if (dictionary.TryGetValue(key, out collection)) {
            bool removed = collection.Remove(value);

            if (collection.Count == 0)
                dictionary.Remove(key);

            return removed;
        }

        return false;
    }

    public static void RemoveByValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value)// where TValue : , IEqualityComparer<TValue>
    {
        var item = dictionary.First(kvp => kvp.Value.Equals(value));

        if (item.Key != null) dictionary.Remove(item.Key);
    }
}