using Dzaba.QueueSimulator.Lib.Model;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.QueueSimulator.Lib.Events;

public interface ISimulationEventQueue
{
    void Run();
    void AddInitAgentQueueEvent(Request request, DateTime time);
    void AddStartRequestQueueEvent(Request request, DateTime time);
    void AddEndRequestQueueEvent(Request request, DateTime time);
    void AddCreateAgentQueueEvent(Request request, DateTime time);
    void AddQueueRequestQueueEvent(RequestConfiguration requestConfiguration, DateTime requestStartTime);
}

internal sealed class SimulationEventQueue : ISimulationEventQueue
{
    private readonly ILogger<SimulationEventQueue> logger;
    private readonly EventQueue eventsQueue = new();
    private readonly IEventHandlers eventHandlers;
    private readonly ISimulationContext simulationContext;

    public SimulationEventQueue(ILogger<SimulationEventQueue> logger,
        IEventHandlers eventHandlers,
        ISimulationContext simulationContext)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(eventHandlers, nameof(eventHandlers));
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));

        this.logger = logger;
        this.eventHandlers = eventHandlers;
        this.simulationContext = simulationContext;
    }

    public void Run()
    {
        while (eventsQueue.Count > 0)
        {
            eventsQueue.Dequeue().Invoke();
        }
    }

    public void AddInitAgentQueueEvent(Request request, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        logger.LogInformation("Adding agent init for {RequestId} [{Request}] for {Time} to the event queue.",
            request.Id, request.RequestConfiguration, time);

        eventsQueue.Enqueue(EventNames.InitAgent, time, e =>
        {
            var payload = new InitAgentEventPayload(e, request);
            var handler = eventHandlers.GetHandler<InitAgentEventPayload>();
            handler.Handle(payload);
        });
    }

    public void AddStartRequestQueueEvent(Request request, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        logger.LogInformation("Adding start request {Request} for {Time} to the event queue.", request.RequestConfiguration, time);

        eventsQueue.Enqueue(EventNames.StartRequest, time, e =>
        {
            var payload = new StartRequestEventPayload(e, request);
            var handler = eventHandlers.GetHandler<StartRequestEventPayload>();
            handler.Handle(payload);
        });
    }

    public void AddEndRequestQueueEvent(Request request, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var payload = simulationContext.Payload;
        var requestConfig = payload.GetRequestConfiguration(request.RequestConfiguration);
        var requestEndTime = time + requestConfig.Duration.Value;

        logger.LogInformation("Adding finishing request {Request} for {Time} to the event queue.",
            request.RequestConfiguration, requestEndTime);

        eventsQueue.Enqueue(EventNames.FinishRequest, requestEndTime, e =>
        {
            var payload = new EndRequestEventPayload(e, request);
            var handler = eventHandlers.GetHandler<EndRequestEventPayload>();
            handler.Handle(payload);
        });
    }

    public void AddCreateAgentQueueEvent(Request request, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        logger.LogInformation("Adding create agent for request {Request} for {Time} to the event queue.",
            request.RequestConfiguration, time);

        eventsQueue.Enqueue(EventNames.CreateAgent, time, e =>
        {
            var payload = new CreateAgentEventPayload(e, request);
            var handler = eventHandlers.GetHandler<CreateAgentEventPayload>();
            handler.Handle(payload);
        });
    }

    public void AddQueueRequestQueueEvent(RequestConfiguration requestConfiguration, DateTime requestStartTime)
    {
        ArgumentNullException.ThrowIfNull(requestConfiguration, nameof(requestConfiguration));

        logger.LogInformation("Adding adding request {Request} for {Time} to the event queue.",
            requestConfiguration.Name, requestStartTime);

        eventsQueue.Enqueue(EventNames.QueueRequest, requestStartTime, e =>
        {
            var payload = new QueueRequestEventPayload(e, requestConfiguration);
            var handler = eventHandlers.GetHandler<QueueRequestEventPayload>();
            handler.Handle(payload);
        });
    }
}
