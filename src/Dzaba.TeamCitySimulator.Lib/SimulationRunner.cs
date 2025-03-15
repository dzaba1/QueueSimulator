using Dzaba.TeamCitySimulator.Lib.Events;
using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TeamCitySimulator.Lib.Queues;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib;

internal sealed class SimulationRunner
{
    private static readonly DateTime StartTime = new DateTime(2025, 1, 1);

    private readonly SimulationPayload simulationPayload;
    private readonly ISimulationValidation simulationValidation;
    private readonly ILogger<SimulationRunner> logger;
    private readonly EventQueue eventsQueue = new();
    private readonly SimulationEvents simulationEvents;
    private readonly BuildsRepository buildRepo;
    private readonly AgentsRepository agentsRepo;

    public SimulationRunner(SimulationSettings simulationSettings,
        ISimulationValidation simulationValidation,
        ILogger<SimulationRunner> logger)
    {
        ArgumentNullException.ThrowIfNull(simulationSettings, nameof(simulationSettings));
        ArgumentNullException.ThrowIfNull(simulationValidation, nameof(simulationValidation));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        this.simulationValidation = simulationValidation;
        this.logger = logger;

        simulationPayload = new SimulationPayload(simulationSettings);
        buildRepo = new BuildsRepository(simulationPayload);
        agentsRepo = new AgentsRepository(simulationPayload);
        simulationEvents = new SimulationEvents(simulationPayload, buildRepo, agentsRepo);
    }

    public IEnumerable<TimeEventData> Run()
    {
        simulationValidation.Validate(simulationPayload);

        InitBuilds();

        while (eventsQueue.Count > 0)
        {
            eventsQueue.Dequeue().Invoke();
        }

        return simulationEvents;
    }

    private void InitBuilds()
    {
        foreach (var queuedBuild in simulationPayload.SimulationSettings.QueuedBuilds)
        {
            var waitTime = simulationPayload.SimulationSettings.SimulationDuration / queuedBuild.BuildsToQueue;
            for (var i = 0; i < queuedBuild.BuildsToQueue; i++)
            {
                var buildStartTime = StartTime + waitTime * i;
                var build = simulationPayload.GetBuildConfiguration(queuedBuild.Name);
                AddQueueBuildQueueEvent(build, buildStartTime);
            }
        }
    }

    private void AddQueueBuildQueueEvent(BuildConfiguration buildConfiguration, DateTime buildStartTime)
    {
        logger.LogInformation("Adding adding build {Build} for {Time} to the event queue.", buildConfiguration.Name, buildStartTime);
        eventsQueue.Enqueue(EventNames.QueueBuild, buildStartTime, e => QueueBuild(e, buildConfiguration));
    }

    private void AddEndBuildQueueEvent(Build build, DateTime currentTime)
    {
        var buildConfig = simulationPayload.GetBuildConfiguration(build.BuildConfiguration);
        var buildEndTime = currentTime + buildConfig.Duration.Value;
        logger.LogInformation("Adding finishing build {Build} for {Time} to the event queue.", build.BuildConfiguration, buildEndTime);
        eventsQueue.Enqueue(EventNames.FinishBuild, buildEndTime, e => FinishBuild(e, build));
    }

    private void AddStartBuildQueueEvent(Build build, DateTime time)
    {
        logger.LogInformation("Adding start build {Build} for {Time} to the event queue.", build.BuildConfiguration, time);
        eventsQueue.Enqueue(EventNames.StartBuild, time, e => StartBuild(e, build));
    }

    private void AddCreateAgentQueueEvent(Build build, DateTime time)
    {
        logger.LogInformation("Adding create agent for {Build} for {Time} to the event queue.", build.BuildConfiguration, time);
        eventsQueue.Enqueue(EventNames.CreateAgent, time, e => CreateAgent(e, build));
    }

    private void AddInitAgentQueueEvent(Build build, DateTime time)
    {
        logger.LogInformation("Adding agent init for [{BuildId}] {Build} for {Time} to the event queue.", build.Id, build.BuildConfiguration, time);
        eventsQueue.Enqueue(EventNames.InitAgent, time, e => InitAgent(e, build));
    }

    private void QueueBuild(EventData eventData, BuildConfiguration buildConfiguration)
    {
        logger.LogInformation("Start queuening a new build {Build}, Current time: {Time}", buildConfiguration.Name, eventData.Time);

        var build = buildRepo.NewBuild(buildConfiguration, eventData.Time);
        if (buildConfiguration.BuildDependencies != null && buildConfiguration.BuildDependencies.Any())
        {
            build.State = BuildState.WaitingForDependencies;

            throw new NotImplementedException();
        }
        else
        {
            AddCreateAgentQueueEvent(build, eventData.Time);
        }

        simulationEvents.AddTimedEventData(eventData, $"Queued a new build [{build.Id}] {build.BuildConfiguration}.");
    }

    private void StartBuild(EventData eventData, Build build)
    {
        logger.LogInformation("Starting the build [{BuildId}] {Build}, Current time: {Time}", build.Id, build.BuildConfiguration, eventData.Time);

        var agent = agentsRepo.GetAgent(build.AgentId.Value);

        agent.State = AgentState.Running;
        build.StartTime = eventData.Time;
        build.State = BuildState.Running;
        AddEndBuildQueueEvent(build, eventData.Time);

        simulationEvents.AddTimedEventData(eventData, $"Started the build [{build.Id}] {build.BuildConfiguration} on agent [{agent.Id}] {agent.AgentConfiguration}.");
    }

    private void FinishBuild(EventData eventData, Build build)
    {
        logger.LogInformation("Start finishing build [{BuildId}] {Build}, Current time: {Time}", build.Id, build.BuildConfiguration, eventData.Time);

        var agent = agentsRepo.GetAgent(build.AgentId.Value);

        agent.State = AgentState.Finished;
        agent.EndTime = eventData.Time;
        build.EndTime = eventData.Time;
        build.State = BuildState.Finished;

        foreach (var scheduledBuild in buildRepo.GetWaitingForAgents())
        {
            AddCreateAgentQueueEvent(scheduledBuild, eventData.Time);
        }

        simulationEvents.AddTimedEventData(eventData, $"Finished the build [{build.Id}] {build.BuildConfiguration} on agent [{agent.Id}] {agent.AgentConfiguration}.");
    }

    private void CreateAgent(EventData eventData, Build build)
    {
        logger.LogInformation("Start creating an agent for build [{BuildId}] {Build}, Current time: {Time}", build.Id, build.BuildConfiguration, eventData.Time);

        var buildConfig = simulationPayload.GetBuildConfiguration(build.BuildConfiguration);
        var eventMsg = "";

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
            var agentConfig = simulationPayload.GetAgentConfiguration(agent.AgentConfiguration);
            eventMsg = $"Created a new agent [{agent.Id}] {agent.AgentConfiguration} for build [{build.Id}] {build.BuildConfiguration}";

            if (agentConfig.InitTime != null)
            {
                AddInitAgentQueueEvent(build, eventData.Time);
            }
            else
            {
                AddStartBuildQueueEvent(build, eventData.Time);
            }
        }
        else
        {
            eventMsg = $"There aren't any agents available for build [{build.Id}] {build.BuildConfiguration}.";
            logger.LogInformation("There aren't any agents available for build [{BuildId}] {Build}.", build.Id, build.BuildConfiguration);
        }

        simulationEvents.AddTimedEventData(eventData, eventMsg);
    }

    private void InitAgent(EventData eventData, Build build)
    {
        var agent = agentsRepo.GetAgent(build.AgentId.Value);

        agent.State = AgentState.Initiating;

        var agentConfig = simulationPayload.GetAgentConfiguration(agent.AgentConfiguration);
        var endTime = eventData.Time + agentConfig.InitTime.Value;

        AddStartBuildQueueEvent(build, endTime);

        simulationEvents.AddTimedEventData(eventData, $"Start initiating agent [{agent.Id}] {agent.AgentConfiguration}.");
    }
}
