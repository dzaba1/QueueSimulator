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
    private readonly SimulationSettings simulationSettings;
    private readonly ISimulationValidation simulationValidation;
    private readonly ILogger<SimulationRunner> logger;
    private readonly DateTime startTime = new DateTime(2025, 1, 1);
    private readonly EventQueue eventsQueue = new();
    private readonly IReadOnlyDictionary<string, BuildConfiguration> buildConfigurationsCached;
    private readonly IReadOnlyDictionary<string, AgentConfiguration> agentConfigurationsCached;
    private readonly List<TimeEventData> timeEvents = new();
    private readonly BuildQueue buildQueue = new();
    private readonly AgentsQueue agentsQueue;

    public SimulationRunner(SimulationSettings simulationSettings,
        ISimulationValidation simulationValidation,
        ILogger<SimulationRunner> logger)
    {
        ArgumentNullException.ThrowIfNull(simulationSettings, nameof(simulationSettings));
        ArgumentNullException.ThrowIfNull(simulationValidation, nameof(simulationValidation));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        this.simulationSettings = simulationSettings;
        this.simulationValidation = simulationValidation;
        this.logger = logger;

        buildConfigurationsCached = simulationSettings.CacheBuildConfiguration();
        agentConfigurationsCached = simulationSettings.CacheAgents();
        agentsQueue = new AgentsQueue(agentConfigurationsCached);
    }

    public IEnumerable<TimeEventData> Run()
    {
        simulationValidation.Validate(buildConfigurationsCached, agentConfigurationsCached, simulationSettings.QueuedBuilds);

        InitBuilds();

        while (eventsQueue.Count > 0)
        {
            eventsQueue.Dequeue().Invoke();
        }

        return timeEvents;
    }

    private void InitBuilds()
    {
        foreach (var queuedBuild in simulationSettings.QueuedBuilds)
        {
            var waitTime = simulationSettings.SimulationDuration / queuedBuild.BuildsToQueue;
            for (var i = 0; i < queuedBuild.BuildsToQueue; i++)
            {
                var buildStartTime = startTime + waitTime * i;
                var build = buildConfigurationsCached[queuedBuild.Name];
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
        var buildConfig = buildConfigurationsCached[build.BuildConfiguration];
        var buildEndTime = currentTime + buildConfig.Duration;
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

    private void AddInitAgentQueueEvent(Agent agent, Build build, DateTime time)
    {
        logger.LogInformation("Adding agent init for [{AgentId}] {Agent} for {Time} to the event queue.", agent.Id, agent.AgentConfiguration, time);
        eventsQueue.Enqueue(EventNames.InitAgent, time, e => InitAgent(e, agent, build));
    }

    private void QueueBuild(EventData eventData, BuildConfiguration buildConfiguration)
    {
        logger.LogInformation("Start queuening a new build {Build}, Current time: {Time}", buildConfiguration.Name, eventData.Time);

        var build = buildQueue.NewBuild(buildConfiguration, eventData.Time);
        AddCreateAgentQueueEvent(build, eventData.Time);

        AddTimedEventData(eventData, $"Queued a new build [{build.Id}] {build.BuildConfiguration}.");
    }

    private void StartBuild(EventData eventData, Build build)
    {
        logger.LogInformation("Starting the build [{BuildId}] {Build}, Current time: {Time}", build.Id, build.BuildConfiguration, eventData.Time);

        var agent = agentsQueue.GetAgent(build.AgentId.Value);

        agent.State = AgentState.Running;
        build.StartTime = eventData.Time;
        build.State = BuildState.Running;
        AddEndBuildQueueEvent(build, eventData.Time);

        AddTimedEventData(eventData, $"Started the build [{build.Id}] {build.BuildConfiguration} on agent [{agent.Id}] {agent.AgentConfiguration}.");
    }

    private void FinishBuild(EventData eventData, Build build)
    {
        logger.LogInformation("Start finishing build [{BuildId}] {Build}, Current time: {Time}", build.Id, build.BuildConfiguration, eventData.Time);

        var agent = agentsQueue.GetAgent(build.AgentId.Value);

        agent.State = AgentState.Finished;
        build.EndTime = eventData.Time;
        build.State = BuildState.Finished;

        AddTimedEventData(eventData, $"Finished the build [{build.Id}] {build.BuildConfiguration} on agent [{agent.Id}] {agent.AgentConfiguration}.");
    }

    private void CreateAgent(EventData eventData, Build build)
    {
        logger.LogInformation("Start creating an agent for build [{BuildId}] {Build}, Current time: {Time}", build.Id, build.BuildConfiguration, eventData.Time);

        var buildConfig = buildConfigurationsCached[build.BuildConfiguration];
        var eventMsg = "";

        if (agentsQueue.TryInitAgent(buildConfig.CompatibleAgents, eventData.Time, out var agent))
        {
            build.AgentId = agent.Id;
            var agentConfig = agentConfigurationsCached[agent.AgentConfiguration];
            eventMsg = $"Created a new agent [{agent.Id}] {agent.AgentConfiguration} for build [{build.Id}] {build.BuildConfiguration}";
            if (agentConfig.InitTime != null)
            {
                AddInitAgentQueueEvent(agent, build, eventData.Time);
            }
            else
            {
                AddStartBuildQueueEvent(build, eventData.Time);
            }
        }
        else
        {
            // There aren't any free agents
            throw new NotImplementedException();
        }

        AddTimedEventData(eventData, eventMsg);
    }

    private void InitAgent(EventData eventData, Agent agent, Build build)
    {
        agent.State = AgentState.Initiating;

        var agentConfig = agentConfigurationsCached[agent.AgentConfiguration];
        var endTime = eventData.Time + agentConfig.InitTime.Value;

        AddStartBuildQueueEvent(build, endTime);

        AddTimedEventData(eventData, $"Start initiating agent [{agent.Id}] {agent.AgentConfiguration}.");
    }

    private void AddTimedEventData(EventData data, string message)
    {
        var timedEvent = new TimeEventData
        {
            Timestamp = data.Time,
            Name = data.Name,
            Message = message,
            QueueLength = buildQueue.GetQueueLength(),
            BuildConfigurationQueues = buildQueue.GroupQueueByBuildConfiguration()
                .Select(g => new NamedQueueData
                {
                    Name = g.Key,
                    Length = g.Value.Length,
                })
                .ToArray(),
            RunningAgents = agentsQueue.GetActiveAgentsCount()
                .Select(g => new NamedQueueData
                {
                    Name = g.Key,
                    Length = g.Value,
                })
                .ToArray(),
            TotalRunningBuilds = buildQueue.GetRunningBuildsCount(),
            RunningBuilds = buildQueue.GroupRunningBuildsByBuildConfiguration()
                .Select(g => new NamedQueueData
                {
                    Name = g.Key,
                    Length = g.Value.Length,
                })
                .ToArray()
        };

        timeEvents.Add(timedEvent);
    }
}
