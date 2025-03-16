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
        return collection.GroupBy(keySelector, comparer)
            .ToDictionary(g => g.Key, g => g.ToArray(), comparer);
    }
}
