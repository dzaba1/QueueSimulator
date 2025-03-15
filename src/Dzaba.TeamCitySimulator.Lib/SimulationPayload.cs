using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib;

internal sealed class SimulationPayload
{
    public SimulationPayload(SimulationSettings simulationSettings)
    {
        ArgumentNullException.ThrowIfNull(simulationSettings, nameof(simulationSettings));

        SimulationSettings = simulationSettings;
        BuildConfigurationsCached = simulationSettings.BuildConfigurations.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
        AgentConfigurationsCached = simulationSettings.Agents.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
    }

    public SimulationSettings SimulationSettings { get; }
    public IReadOnlyDictionary<string, BuildConfiguration> BuildConfigurationsCached { get; }
    public IReadOnlyDictionary<string, AgentConfiguration> AgentConfigurationsCached { get; }

    public BuildConfiguration GetBuildConfiguration(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return BuildConfigurationsCached[name];
    }

    public AgentConfiguration GetAgentConfiguration(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return AgentConfigurationsCached[name];
    }
}
