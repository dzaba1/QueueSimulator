using System;
using System.Collections.Generic;

namespace Dzaba.TeamCitySimulator.Lib.Events;

public sealed class EventQueue
{
    private readonly PriorityQueue<Event, DateTime> queue = new();

    public void Enqueue(string name, DateTime time, Action<EventData> action)
    {
        var @event = new Event(name, time, action);
        queue.Enqueue(@event, time);
    }

    public void Enqueue(EventData eventData, Action<EventData> action)
    {
        var @event = new Event(eventData, action);
        queue.Enqueue(@event, @event.Data.Time);
    }

    public Event Dequeue()
    {
        return queue.Dequeue();
    }

    public int Count => queue.Count;
}
