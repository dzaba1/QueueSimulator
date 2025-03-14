using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;

namespace Dzaba.TeamCitySimulator.Lib;

internal sealed class SimulationRunner
{
    private readonly SimulationSettings simulationSettings;
    private readonly ISimulationValidation simulationValidation;
    private readonly DateTime startDate = new DateTime(2025, 1, 1);
    private readonly IReadOnlyDictionary<string, Build> buildsCached;
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

    public IEnumerable<EventData> Run()
    {
        simulationValidation.Validate(buildsCached, agentsCached, simulationSettings.QueuedBuilds);

        throw new NotImplementedException();
    }
}
