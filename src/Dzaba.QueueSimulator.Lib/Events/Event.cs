using System;

namespace Dzaba.QueueSimulator.Lib.Events;

public sealed class Event
{
    public Event(string name, DateTime time, Action<EventData> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentNullException.ThrowIfNull(action, nameof(action));

        Data = new EventData(name, time);
        Action = action;
    }

    public Event(EventData eventData, Action<EventData> action)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

        Data = eventData;
        Action = action;
    }

    public EventData Data { get; }
    public Action<EventData> Action { get; }

    public void Invoke()
    {
        Action(Data);
    }
}
