using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dzaba.QueueSimulator.Lib.Events;

internal interface IEventHandlers
{
    IEventHandler<T> GetHandler<T>(string eventName);
}

internal sealed class EventHandlers : IEventHandlers
{
    private readonly IServiceProvider container;

    public EventHandlers(IServiceProvider container)
    {
        ArgumentNullException.ThrowIfNull(container, nameof(container));

        this.container = container;
    }

    public IEventHandler<T> GetHandler<T>(string eventName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));

        return container.GetRequiredKeyedService<IEventHandler<T>>(eventName);
    }
}
