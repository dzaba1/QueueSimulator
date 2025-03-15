using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TeamCitySimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.TeamCitySimulator.Lib.Events;

internal sealed class CreateAgentEventPayload : EventDataPayload
{
    public CreateAgentEventPayload(EventData eventData, Build build)
        : base(eventData)
    {
        ArgumentNullException.ThrowIfNull(build, nameof(build));

        Build = build;
    }

    public Build Build { get; }
}

internal sealed class CreateAgentEventHandler : EventHandler<CreateAgentEventPayload>
{
    private readonly ILogger<CreateAgentEventHandler> logger;
    private readonly ISimulationContext simulationContext;
    private readonly IAgentsRepository agentsRepo;
    private readonly ISimulationEventQueue eventsQueue;

    public CreateAgentEventHandler(ISimulationEvents simulationEvents,
        ILogger<CreateAgentEventHandler> logger,
        ISimulationContext simulationContext,
        IAgentsRepository agentsRepo,
        ISimulationEventQueue eventsQueue)
        : base(simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));
        ArgumentNullException.ThrowIfNull(agentsRepo, nameof(agentsRepo));
        ArgumentNullException.ThrowIfNull(eventsQueue, nameof(eventsQueue));

        this.logger = logger;
        this.simulationContext = simulationContext;
        this.agentsRepo = agentsRepo;
        this.eventsQueue = eventsQueue;
    }

    protected override string OnHandle(CreateAgentEventPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var build = payload.Build;
        var eventData = payload.EventData;

        logger.LogInformation("Start creating an agent for build [{BuildId}] {Build}, Current time: {Time}", build.Id, build.BuildConfiguration, eventData.Time);

        var buildConfig = simulationContext.Payload.GetBuildConfiguration(build.BuildConfiguration);

        if (buildConfig.IsComposite)
        {
            throw new InvalidOperationException($"Build [{build.Id}] {buildConfig.Name} is composite. It can't be ran on agent.");
        }

        if (build.AgentId != null)
        {
            throw new InvalidOperationException($"Agent with ID {build.AgentId} was already assigned to build with ID {build.Id}.");
        }

        build.State = BuildState.WaitingForAgent;

        if (agentsRepo.TryInitAgent(buildConfig.CompatibleAgents, eventData.Time, out var agent))
        {
            build.AgentId = agent.Id;
            var agentConfig = simulationContext.Payload.GetAgentConfiguration(agent.AgentConfiguration);

            if (agentConfig.InitTime != null)
            {
                eventsQueue.AddInitAgentQueueEvent(build, eventData.Time);
            }
            else
            {
                eventsQueue.AddStartBuildQueueEvent(build, eventData.Time);
            }

            return $"Created a new agent [{agent.Id}] {agent.AgentConfiguration} for build [{build.Id}] {build.BuildConfiguration}";
        }
        else
        {
            logger.LogInformation("There aren't any agents available for build [{BuildId}] {Build}.", build.Id, build.BuildConfiguration);
            return $"There aren't any agents available for build [{build.Id}] {build.BuildConfiguration}.";
        }
    }
}
