using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib;

public static class Extensions
{
    public static IReadOnlyDictionary<TKey, TValue[]> GroupByToArrayDict<TKey, TValue>(this IEnumerable<TValue> collection,
        Func<TValue, TKey> keySelector,
        IEqualityComparer<TKey> comparer = null)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));

        return collection.GroupBy(keySelector, comparer)
            .ToDictionary(g => g.Key, g => g.ToArray(), comparer);
    }

    public static IEnumerable<T> ForEachLazy<T>(this IEnumerable<T> collection, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));
        ArgumentNullException.ThrowIfNull(action, nameof(action));

        return collection.Select(o =>
        {
            action(o);
            return o;
        });
    }
}
