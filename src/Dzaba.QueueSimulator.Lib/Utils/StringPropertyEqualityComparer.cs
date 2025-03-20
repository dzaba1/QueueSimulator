using System;

namespace Dzaba.QueueSimulator.Lib.Utils;

public class StringPropertyEqualityComparer<T> : OnePropertyComparer<T, string>
{
    public StringPropertyEqualityComparer(Func<T, string> keySelector)
        : this(keySelector, StringComparer.CurrentCulture)
    {

    }

    public StringPropertyEqualityComparer(Func<T, string> keySelector,
        StringComparer stringComparer)
        : base(keySelector, stringComparer)
    {

    }
}
