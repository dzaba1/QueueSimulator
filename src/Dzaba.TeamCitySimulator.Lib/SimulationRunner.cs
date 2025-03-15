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

        throw new NotImplementedException();
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
                logger.LogInformation("Initiating build {Build} for {Time}", build.Name, buildStartTime);
            }
        }
    }
}
