using Dzaba.QueueSimulator.Lib.Model;
using System;

namespace Dzaba.QueueSimulator.Lib;

public sealed class SimulationPayload
{
    public SimulationPayload(SimulationSettings simulationSettings)
    {
        ArgumentNullException.ThrowIfNull(simulationSettings, nameof(simulationSettings));

        SimulationSettings = simulationSettings;
        RequestConfigurations = new EntityPayload<RequestConfiguration, string>(simulationSettings.RequestConfigurations,
            r => r.Name,
            simulationSettings.ReportSettings.RequestConfigurationsToObserve,
            StringComparer.OrdinalIgnoreCase);
        AgentConfigurations = new EntityPayload<AgentConfiguration, string>(simulationSettings.Agents,
            r => r.Name,
            simulationSettings.ReportSettings.AgentConfigurationsToObserve,
            StringComparer.OrdinalIgnoreCase);
    }

    public SimulationSettings SimulationSettings { get; }
    public EntityPayload<RequestConfiguration, string> RequestConfigurations { get; }
    public EntityPayload<AgentConfiguration, string> AgentConfigurations { get; }
}
