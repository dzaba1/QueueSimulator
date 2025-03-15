using System;

namespace Dzaba.TeamCitySimulator.Lib.Events;

internal interface IEventHandlers
{
    IEventHandler<T> GetHandler<T>() where T : EventDataPayload;
}

internal sealed class EventHandlers : IEventHandlers
{
    private readonly IEventHandler<InitAgentEventPayload> initAgentEventHandler;

    public EventHandlers(IEventHandler<InitAgentEventPayload> initAgentEventHandler)
    {
        ArgumentNullException.ThrowIfNull(initAgentEventHandler, nameof(initAgentEventHandler));

        this.initAgentEventHandler = initAgentEventHandler;
    }

    public IEventHandler<T> GetHandler<T>() where T : EventDataPayload
    {
        var type = typeof(T);

        if (type == typeof(InitAgentEventPayload))
        {
            return (IEventHandler<T>)initAgentEventHandler;
        }
        else
        {
            throw new ArgumentOutOfRangeException($"Unknown payload type {type}");
        }
    }
}
