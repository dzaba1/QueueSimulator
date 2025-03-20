using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.QueueSimulator.Lib.Events;

internal sealed class AgentInitiatedEventHandler : EventHandler<Request>
{
    private readonly ILogger<AgentInitiatedEventHandler> logger;
    private readonly IAgentsRepository agentsRepo;
    private readonly ISimulationEventQueue eventQueue;

    public AgentInitiatedEventHandler(ISimulationEvents simulationEvents,
        ILogger<AgentInitiatedEventHandler> logger,
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
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));
        
        var request = payload;

        var agent = agentsRepo.GetAgent(request.AgentId.Value);
        agent.State = AgentState.Initiated;

        logger.LogInformation("Agent {AgentId} [{Agent}] initiated for request {RequestdId} [{Request}] for {Time}.",
            agent.Id,
            agent.AgentConfiguration,
            request.Id,
            request.RequestConfiguration,
            eventData.Time);

        eventQueue.AddStartRequestQueueEvent(request, eventData.Time);

        return $"Initiating agent {agent.Id} [{agent.AgentConfiguration}] finished.";
    }
}
