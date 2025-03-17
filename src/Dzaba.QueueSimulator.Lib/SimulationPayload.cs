using Dzaba.QueueSimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib;

public sealed class SimulationPayload
{
    public SimulationPayload(SimulationSettings simulationSettings)
    {
        ArgumentNullException.ThrowIfNull(simulationSettings, nameof(simulationSettings));

        SimulationSettings = simulationSettings;
        RequestConfigurationsCached = simulationSettings.RequestConfigurations.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
        AgentConfigurationsCached = simulationSettings.Agents.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
    }

    public SimulationSettings SimulationSettings { get; }
    public IReadOnlyDictionary<string, RequestConfiguration> RequestConfigurationsCached { get; }
    public IReadOnlyDictionary<string, AgentConfiguration> AgentConfigurationsCached { get; }

    public RequestConfiguration GetRequestConfiguration(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return RequestConfigurationsCached[name];
    }

    public AgentConfiguration GetAgentConfiguration(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return AgentConfigurationsCached[name];
    }
}
