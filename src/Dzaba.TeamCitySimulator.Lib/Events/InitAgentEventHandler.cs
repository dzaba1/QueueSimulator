using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TeamCitySimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.TeamCitySimulator.Lib.Events;

internal sealed class InitAgentEventPayload : EventDataPayload
{
    public InitAgentEventPayload(EventData eventData, Build build)
        : base(eventData)
    {
        ArgumentNullException.ThrowIfNull(build, nameof(build));

        Build = build;
    }

    public Build Build { get; }
}

internal sealed class InitAgentEventHandler : EventHandler<InitAgentEventPayload>
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

    protected override string OnHandle(InitAgentEventPayload payload)
    {
        logger.LogInformation("Adding agent init for [{BuildId}] {Build} for {Time} to the event queue.",
            payload.Build.Id,
            payload.Build.BuildConfiguration, payload.EventData.Time);

        var agent = agentsRepo.GetAgent(payload.Build.AgentId.Value);

        agent.State = AgentState.Initiating;

        var agentConfig = simulationContext.Payload.GetAgentConfiguration(agent.AgentConfiguration);
        var endTime = payload.EventData.Time + agentConfig.InitTime.Value;

        eventQueue.AddStartBuildQueueEvent(payload.Build, endTime);

        return $"Start initiating agent [{agent.Id}] {agent.AgentConfiguration}.";
    }
}
