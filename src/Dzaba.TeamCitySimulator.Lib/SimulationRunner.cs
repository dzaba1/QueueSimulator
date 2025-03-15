using Dzaba.TeamCitySimulator.Lib.Events;
using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;

namespace Dzaba.TeamCitySimulator.Lib;

internal sealed class SimulationRunner
{
    private readonly SimulationSettings simulationSettings;
    private readonly ISimulationValidation simulationValidation;
    private readonly DateTime startTime = new DateTime(2025, 1, 1);
    private readonly EventQueue eventsQueue = new();
    private readonly IReadOnlyDictionary<string, BuildConfiguration> buildsCached;
    private readonly IReadOnlyDictionary<string, Agent> agentsCached;

    public SimulationRunner(SimulationSettings simulationSettings,
        ISimulationValidation simulationValidation)
    {
        ArgumentNullException.ThrowIfNull(simulationSettings, nameof(simulationSettings));
        ArgumentNullException.ThrowIfNull(simulationValidation, nameof(simulationValidation));

        this.simulationSettings = simulationSettings;
        this.simulationValidation = simulationValidation;

        buildsCached = simulationSettings.CacheBuildConfiguration();
        agentsCached = simulationSettings.CacheAgents();
    }

    public IEnumerable<TimeEventData> Run()
    {
        simulationValidation.Validate(buildsCached, agentsCached, simulationSettings.QueuedBuilds);

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
                var build = buildsCached[queuedBuild.Name];
            }
        }
    }
}
