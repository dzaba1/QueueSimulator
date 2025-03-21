using System;

namespace Dzaba.QueueSimulator.Lib.Events;

public sealed class EventData
{
    public EventData(string name, DateTimeOffset time)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        Name = name;
        Time = time;
    }

    public string Name { get; }
    public DateTimeOffset Time { get; }
}
