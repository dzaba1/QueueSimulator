using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Utils
{
    public sealed class StringArrayComparer : IEqualityComparer<string[]>
    {
        private readonly StringComparer stringComparer;

        public StringArrayComparer(StringComparer stringComparer)
        {
            ArgumentNullException.ThrowIfNull(stringComparer, nameof(stringComparer));

            this.stringComparer = stringComparer;
        }

        public bool Equals(string[] x, string[] y)
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

            if (x.Length != y.Length)
            {
                return false;
            }

            var xOrdered = x.OrderBy(s => s).ToArray();
            var yOrdered = x.OrderBy(s => s).ToArray();

            for (int i = 0; i < x.Length; i++)
            {
                var xStr = xOrdered[i];
                var yStr = yOrdered[i];

                if (!stringComparer.Equals(xStr, yStr))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode([DisallowNull] string[] obj)
        {
            unchecked
            {
                var ordered = obj.OrderBy(s => s).ToArray();
                var result = 0;
                foreach (var str in ordered)
                {
                    result ^= stringComparer.GetHashCode(str);
                }
                return result;
            }
        }
    }
}
