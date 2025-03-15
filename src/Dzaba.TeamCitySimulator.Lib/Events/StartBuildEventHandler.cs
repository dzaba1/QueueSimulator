using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TeamCitySimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.TeamCitySimulator.Lib.Events;

internal sealed class StartBuildEventPayload : EventDataPayload
{
    public StartBuildEventPayload(EventData eventData, Build build) : base(eventData)
    {
        ArgumentNullException.ThrowIfNull(build, nameof(build));

        Build = build;
    }

    public Build Build { get; }
}

internal sealed class StartBuildEventHandler : EventHandler<StartBuildEventPayload>
{
    private readonly ILogger<StartBuildEventHandler> logger;
    private readonly IAgentsRepository agentsRepo;
    private readonly ISimulationEventQueue eventQueue;

    public StartBuildEventHandler(ISimulationEvents simulationEvents,
        ILogger<StartBuildEventHandler> logger,
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

    protected override string OnHandle(StartBuildEventPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        logger.LogInformation("Starting the build [{BuildId}] {Build}, Current time: {Time}",
            payload.Build.Id,
            payload.Build.BuildConfiguration,
            payload.EventData.Time);

        var agent = agentsRepo.GetAgent(payload.Build.AgentId.Value);

        agent.State = AgentState.Running;
        payload.Build.StartTime = payload.EventData.Time;
        payload.Build.State = BuildState.Running;

        eventQueue.AddEndBuildQueueEvent(payload.Build, payload.EventData.Time);

        return $"Started the build [{payload.Build.Id}] {payload.Build.BuildConfiguration} on agent [{agent.Id}] {agent.AgentConfiguration}.";
    }
}
