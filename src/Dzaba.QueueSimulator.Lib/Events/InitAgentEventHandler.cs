using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.QueueSimulator.Lib.Events;

internal sealed class InitAgentEventHandler : EventHandler<Request>
{
    private readonly IAgentsRepository agentsRepo;
    private readonly ISimulationContext simulationContext;
    private readonly ISimulationEventQueue eventQueue;
    private readonly ILogger<InitAgentEventHandler> logger;

    public InitAgentEventHandler(ISimulationEvents simulationEvents,
        IAgentsRepository agentsRepo,
        ISimulationContext simulationContext,
        ISimulationEventQueue eventQueue,
        ILogger<InitAgentEventHandler> logger)
        : base(simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(agentsRepo, nameof(agentsRepo));
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));
        ArgumentNullException.ThrowIfNull(eventQueue, nameof(eventQueue));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        this.agentsRepo = agentsRepo;
        this.simulationContext = simulationContext;
        this.eventQueue = eventQueue;
        this.logger = logger;
    }

    protected override string OnHandle(EventData eventData, Request payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var request = payload;

        logger.LogInformation("Adding agent init for request {RequestdId} [{Request}] for {Time} to the event queue.",
            request.Id,
            request.RequestConfiguration, eventData.Time);

        var agent = agentsRepo.GetAgent(request.AgentId.Value);

        agent.State = AgentState.Initiating;

        var agentConfig = simulationContext.Payload.GetAgentConfiguration(agent.AgentConfiguration);

        var endTime = eventData.Time;
        if (agentConfig.InitTime != null)
        {
            endTime = eventData.Time + agentConfig.InitTime.Value;
        }

        eventQueue.AddStartRequestQueueEvent(request, endTime);

        return $"Start initiating agent {agent.Id} [{agent.AgentConfiguration}].";
    }
}
