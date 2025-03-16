using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dzaba.QueueSimulator.Lib.Model
{
    public sealed class StringPropertyEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly StringComparer stringComparer;
        private readonly Func<T, string> keySelector;

        public StringPropertyEqualityComparer(Func<T, string> keySelector)
            : this(keySelector, StringComparer.CurrentCulture)
        {

        }

        public StringPropertyEqualityComparer(Func<T, string> keySelector,
            StringComparer stringComparer)
        {
            ArgumentNullException.ThrowIfNull(keySelector, nameof(keySelector));
            ArgumentNullException.ThrowIfNull(stringComparer, nameof(stringComparer));

            this.keySelector = keySelector;
            this.stringComparer = stringComparer;
        }

        public bool Equals(T? x, T? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y != null)
            {
                return false;
            }

            if (x != null || y == null)
            {
                return false;
            }

            return stringComparer.Equals(keySelector(x), keySelector(y));
        }

        public int GetHashCode([DisallowNull] T obj)
        {
            return stringComparer.GetHashCode(keySelector(obj));
        }
    }
}
