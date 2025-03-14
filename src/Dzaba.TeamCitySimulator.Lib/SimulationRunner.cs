using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib;

internal sealed class SimulationRunner
{
    private readonly SimulationSettings simulationSettings;
    private readonly DateTime startDate = new DateTime(2025, 1, 1);
    private readonly IReadOnlyDictionary<string, Build> buildsCached;
    private readonly IReadOnlyDictionary<string, Agent> agentsCached;

    public SimulationRunner(SimulationSettings simulationSettings)
    {
        ArgumentNullException.ThrowIfNull(simulationSettings, nameof(simulationSettings));

        this.simulationSettings = simulationSettings;

        buildsCached = simulationSettings.BuildConfigurations.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
        agentsCached = simulationSettings.Agents.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<EventData> Run()
    {
        throw new NotImplementedException();
    }
}
