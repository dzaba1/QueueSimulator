using System;

namespace Dzaba.QueueSimulator.Lib.Events;

internal interface IEventHandler<T>
    where T : EventDataPayload
{
    void Handle(T payload);
}

internal abstract class EventHandler<T> : IEventHandler<T>
    where T : EventDataPayload
{
    private readonly ISimulationEvents simulationEvents;

    protected EventHandler(ISimulationEvents simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(simulationEvents, nameof(simulationEvents));

        this.simulationEvents = simulationEvents;
    }

    public void Handle(T payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var msg = OnHandle(payload);

        simulationEvents.AddTimedEventData(payload.EventData, msg);
    }

    protected abstract string OnHandle(T payload);
}
