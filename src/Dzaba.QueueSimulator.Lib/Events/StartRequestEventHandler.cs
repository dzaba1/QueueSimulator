using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.QueueSimulator.Lib.Events;

internal sealed class StartRequestEventHandler : EventHandler<Request>
{
    private readonly ILogger<StartRequestEventHandler> logger;
    private readonly IAgentsRepository agentsRepo;
    private readonly ISimulationEventQueue eventQueue;

    public StartRequestEventHandler(ISimulationEvents simulationEvents,
        ILogger<StartRequestEventHandler> logger,
        IAgentsRepository agentsRepo,
        ISimulationEventQueue eventQueue)
        : base(simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(agentsRepo, nameof(agentsRepo));
        ArgumentNullException.ThrowIfNull(eventQueue, nameof(eventQueue));

        this.logger = logger;
        this.agentsRepo = agentsRepo;
        this.eventQueue = eventQueue;
    }

    protected override string OnHandle(EventData eventData, Request payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var request = payload;

        logger.LogInformation("Starting the request {RequestId} [{Request}], Current time: {Time}",
            request.Id,
            request.RequestConfiguration,
            eventData.Time);

        var agent = agentsRepo.GetAgent(request.AgentId.Value);

        agent.State = AgentState.Running;
        request.StartTime = eventData.Time;
        request.State = RequestState.Running;

        eventQueue.AddEndRequestQueueEvent(request, eventData.Time);

        return $"Started the request {request.Id} [{request.RequestConfiguration}] on agent {agent.Id} [{agent.AgentConfiguration}].";
    }
}
