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

    public static TimeSpan Average<T>(this IEnumerable<T> collection, Func<T, TimeSpan> selector)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));
        ArgumentNullException.ThrowIfNull(selector, nameof(selector));

        var sum = TimeSpan.Zero;
        var count = 0;
        foreach (var item in collection)
        {
            sum += selector(item);
            count++;
        }

        if (count > 0)
        {
            return sum / count;
        }
        return TimeSpan.Zero;
    }
}
