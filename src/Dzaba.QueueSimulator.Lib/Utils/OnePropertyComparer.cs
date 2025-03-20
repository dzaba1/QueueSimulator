using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dzaba.QueueSimulator.Lib.Utils;

public class OnePropertyComparer<T, TKey> : IEqualityComparer<T>
{
    private readonly IEqualityComparer<TKey> keyComparer;
    private readonly Func<T, TKey> keySelector;

    public OnePropertyComparer(Func<T, TKey> keySelector)
        : this(keySelector, EqualityComparer<TKey>.Default)
    {

    }

    public OnePropertyComparer(Func<T, TKey> keySelector,
        IEqualityComparer<TKey> keyComparer)
    {
        ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
        ArgumentNullException.ThrowIfNull(keyComparer, nameof(keyComparer));

        this.keySelector = keySelector;
        this.keyComparer = keyComparer;
    }

    public bool Equals(T x, T y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x == null && y != null)
        {
            return false;
        }

        if (x != null && y == null)
        {
            return false;
        }

        return keyComparer.Equals(keySelector(x), keySelector(y));
    }

    public int GetHashCode([DisallowNull] T obj)
    {
        return keyComparer.GetHashCode(keySelector(obj));
    }
}
