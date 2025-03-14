using System;

namespace Dzaba.TeamCitySimulator.Lib.Events;

public sealed class Event
{
    public Event(string name, DateTime time, Action<Event> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentNullException.ThrowIfNull(action, nameof(action));

        Name = name;
        Time = time;
        Action = action;
    }

    public string Name { get; }
    public DateTime Time { get; }
    public Action<Event> Action { get; }

    public void Invoke()
    {
        Action(this);
    }
}
