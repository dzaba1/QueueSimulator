using Dzaba.TeamCitySimulator.Lib.Events;
using Dzaba.TeamCitySimulator.Lib.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Dzaba.TeamCitySimulator.Lib;

internal sealed class SimulationRunner
{
    private readonly SimulationSettings simulationSettings;
    private readonly ISimulationValidation simulationValidation;
    private readonly ILogger<SimulationRunner> logger;
    private readonly DateTime startTime = new DateTime(2025, 1, 1);
    private readonly EventQueue eventsQueue = new();
    private readonly IReadOnlyDictionary<string, BuildConfiguration> buildConfigurationsCached;
    private readonly IReadOnlyDictionary<string, Agent> agentsCached;
    private readonly List<TimeEventData> timeEvents = new();

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
        agentsCached = simulationSettings.CacheAgents();
    }

    public IEnumerable<TimeEventData> Run()
    {
        simulationValidation.Validate(buildConfigurationsCached, agentsCached, simulationSettings.QueuedBuilds);

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
                AddStartBuildQueueEvent(build, buildStartTime);
            }
        }
    }

    private void AddStartBuildQueueEvent(BuildConfiguration buildConfiguration, DateTime buildStartTime)
    {
        logger.LogInformation("Adding build {Build} for {Time} to the event queue.", buildConfiguration.Name, buildStartTime);
        eventsQueue.Enqueue("QueueBuild", buildStartTime, e => QueueBuild(e, buildConfiguration));
    }

    private void QueueBuild(EventData eventData, BuildConfiguration buildConfiguration)
    {
        logger.LogInformation("Start queuening a new build {Build}, Current time: {Time}", buildConfiguration.Name, eventData.Time);

        AddTimedEventData(eventData);
    }

    private void AddTimedEventData(EventData data)
    {
        var timedEvent = new TimeEventData
        {
            Timestamp = data.Time,
            Name = data.Name,
        };

        timeEvents.Add(timedEvent);
    }
}
