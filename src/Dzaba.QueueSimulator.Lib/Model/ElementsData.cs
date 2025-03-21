using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Model;

[DebuggerDisplay("{Name}: {Value}")]
public sealed class NamedQueueData<T>
{
    public string Name { get; set; }
    public T Value { get; set; }
}

public sealed class ElementsData<T>
{
    public T Total { get; set; }
    public NamedQueueData<T>[] Grouped { get; set; }

    public IReadOnlyDictionary<string, T> ToDictionary()
    {
        return Grouped.ToDictionary(g => g.Name, g => g.Value, StringComparer.OrdinalIgnoreCase);
    }
}
