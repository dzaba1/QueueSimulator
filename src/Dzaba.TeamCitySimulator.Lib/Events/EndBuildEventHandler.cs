using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TeamCitySimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.TeamCitySimulator.Lib.Events;

internal sealed class EndBuildEventPayload : EventDataPayload
{
    public EndBuildEventPayload(EventData eventData, Build build)
        : base(eventData)
    {
        ArgumentNullException.ThrowIfNull(build, nameof(build));

        Build = build;
    }

    public Build Build { get; }
}

internal sealed class EndBuildEventHandler : EventHandler<EndBuildEventPayload>
{
    private readonly ILogger<EndBuildEventHandler> logger;
    private readonly IAgentsRepository agentsRepo;
    private readonly IBuildsRepository buildRepo;
    private readonly ISimulationEventQueue eventQueue;

    public EndBuildEventHandler(ISimulationEvents simulationEvents,
        ILogger<EndBuildEventHandler> logger,
        IAgentsRepository agentsRepo,
        IBuildsRepository buildRepo,
        ISimulationEventQueue eventQueue)
        : base(simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(agentsRepo, nameof(agentsRepo));
        ArgumentNullException.ThrowIfNull(buildRepo, nameof(buildRepo));
        ArgumentNullException.ThrowIfNull(eventQueue, nameof(eventQueue));

        this.logger = logger;
        this.agentsRepo = agentsRepo;
        this.buildRepo = buildRepo;
        this.eventQueue = eventQueue;
    }

    protected override string OnHandle(EndBuildEventPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var build = payload.Build;
        var eventData = payload.EventData;

        logger.LogInformation("Start finishing build [{BuildId}] {Build}, Current time: {Time}", build.Id, build.BuildConfiguration, eventData.Time);

        var agent = agentsRepo.GetAgent(build.AgentId.Value);

        agent.State = AgentState.Finished;
        agent.EndTime = eventData.Time;
        build.EndTime = eventData.Time;
        build.State = BuildState.Finished;

        foreach (var scheduledBuild in buildRepo.GetWaitingForAgents())
        {
            eventQueue.AddCreateAgentQueueEvent(scheduledBuild, eventData.Time);
        }

        return $"Finished the build [{build.Id}] {build.BuildConfiguration} on agent [{agent.Id}] {agent.AgentConfiguration}.";
    }
}
