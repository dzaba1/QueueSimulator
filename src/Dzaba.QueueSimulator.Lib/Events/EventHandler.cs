using System;

namespace Dzaba.QueueSimulator.Lib.Events;

internal interface IEventHandler<T>
{
    void Handle(EventData eventData, T payload);
}

internal abstract class EventHandler<T> : IEventHandler<T>
{
    private readonly ISimulationEvents simulationEvents;

    protected EventHandler(ISimulationEvents simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(simulationEvents, nameof(simulationEvents));

        this.simulationEvents = simulationEvents;
    }

    public void Handle(EventData eventData, T payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var msg = OnHandle(eventData, payload);

        simulationEvents.AddTimedEventData(eventData, msg);
    }

    protected abstract string OnHandle(EventData eventData, T payload);
}
