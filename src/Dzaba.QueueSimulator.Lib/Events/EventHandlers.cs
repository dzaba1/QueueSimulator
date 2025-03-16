using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dzaba.QueueSimulator.Lib.Events;

internal interface IEventHandlers
{
    IEventHandler<T> GetHandler<T>() where T : EventDataPayload;
}

internal sealed class EventHandlers : IEventHandlers
{
    private readonly IServiceProvider container;

    public EventHandlers(IServiceProvider container)
    {
        ArgumentNullException.ThrowIfNull(container, nameof(container));

        this.container = container;
    }

    public IEventHandler<T> GetHandler<T>() where T : EventDataPayload
    {
        return container.GetRequiredService<IEventHandler<T>>();
    }
}
