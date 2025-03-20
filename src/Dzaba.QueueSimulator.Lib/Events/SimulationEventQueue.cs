using Dzaba.QueueSimulator.Lib.Model;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.QueueSimulator.Lib.Events;

internal interface ISimulationEventQueue
{
    void Run();
    void AddInitAgentQueueEvent(Request request, DateTime time);
    void AddAgentInitedQueueEvent(Request request, DateTime time);
    void AddStartRequestQueueEvent(Request request, DateTime time);
    void AddEndRequestQueueEvent(Request request, DateTime time);
    void AddCreateAgentQueueEvent(Request[] request, DateTime time);
    void AddQueueRequestQueueEvent(QueueRequestEventPayload payload, DateTime requestStartTime);
}

internal sealed class SimulationEventQueue : ISimulationEventQueue
{
    private readonly ILogger<SimulationEventQueue> logger;
    private readonly EventQueue eventsQueue = new();
    private readonly IEventHandlers eventHandlers;

    public SimulationEventQueue(ILogger<SimulationEventQueue> logger,
        IEventHandlers eventHandlers)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(eventHandlers, nameof(eventHandlers));

        this.logger = logger;
        this.eventHandlers = eventHandlers;
    }

    public void Run()
    {
        while (eventsQueue.Count > 0)
        {
            eventsQueue.Dequeue().Invoke();
        }
    }

    private void Enqueue<T>(string eventName, DateTime time, T payload)
    {
        eventsQueue.Enqueue(eventName, time, e =>
        {
            var handler = eventHandlers.GetHandler<T>(eventName);
            handler.Handle(e, payload);
        });
    }

    public void AddInitAgentQueueEvent(Request request, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        logger.LogInformation("Adding agent init for {RequestId} [{Request}] for {Time} to the event queue.",
            request.Id, request.RequestConfiguration, time);

        Enqueue(EventNames.InitAgent, time, request);
    }

    public void AddStartRequestQueueEvent(Request request, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        logger.LogInformation("Adding start request {RequestId} [{Request}] for {Time} to the event queue.", request.Id, request.RequestConfiguration, time);

        Enqueue(EventNames.StartRequest, time, request);
    }

    public void AddEndRequestQueueEvent(Request request, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        logger.LogInformation("Adding finishing request {RequestId} [{Request}] for {Time} to the event queue.",
            request.Id, request.RequestConfiguration, time);

        Enqueue(EventNames.FinishRequest, time, request);
    }

    public void AddCreateAgentQueueEvent(Request[] request, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (request.Length == 0)
        {
            return;
        }

        logger.LogInformation("Adding create agent for requests for {Time} to the event queue.", time);

        Enqueue(EventNames.CreateAgent, time, request);
    }

    public void AddQueueRequestQueueEvent(QueueRequestEventPayload payload, DateTime requestStartTime)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        logger.LogInformation("Adding adding request {Request} for {Time} to the event queue.",
            payload.RequestConfiguration.Name, requestStartTime);

        Enqueue(EventNames.QueueRequest, requestStartTime, payload);
    }

    public void AddAgentInitedQueueEvent(Request request, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        logger.LogInformation("Adding agent initiated for request {RequestId} [{Request}] for {Time} to the event queue.",
            request.Id, request.RequestConfiguration, time);

        Enqueue(EventNames.AgentInitiated, time, request);
    }
}
