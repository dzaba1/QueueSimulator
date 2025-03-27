using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib;

public sealed class EntityPayload<T, TKey>
{
    public EntityPayload(IEnumerable<T> entities,
        Func<T, TKey> keySelector,
        IEnumerable<TKey> toObserve,
        IEqualityComparer<TKey> comparer)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        ArgumentNullException.ThrowIfNull(comparer, nameof(comparer));

        Cache = entities.ToDictionary(keySelector, x => x, comparer);

        if (toObserve != null)
        {
            ToObserve = new HashSet<TKey>(toObserve, comparer);
        }
    }

    public IReadOnlyDictionary<TKey, T> Cache { get; }
    public HashSet<TKey> ToObserve { get; }

    public T GetEntity(TKey id)
    {
        return Cache[id];
    }

    public bool ShouldObserve(TKey id)
    {
        if (ToObserve == null)
        {
            return true;
        }

        return ToObserve.Contains(id);
    }
}
