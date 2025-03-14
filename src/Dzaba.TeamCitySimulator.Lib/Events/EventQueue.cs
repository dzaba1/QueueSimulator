using System;
using System.Collections.Generic;

namespace Dzaba.TeamCitySimulator.Lib.Events;

public sealed class EventQueue
{
    private readonly PriorityQueue<Event, DateTime> queue = new();

    public void Enqueue(string name, DateTime time, Action<Event> action)
    {
        var @event = new Event(name, time, action);
        queue.Enqueue(@event, time);
    }

    public Event Dequeue()
    {
        return queue.Dequeue();
    }
}
