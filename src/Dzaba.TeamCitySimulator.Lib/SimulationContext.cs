using Dzaba.TeamCitySimulator.Lib.Model;
using System;

namespace Dzaba.TeamCitySimulator.Lib;

internal interface ISimulationContext
{
    public SimulationPayload Payload { get; }
    void SetSettings(SimulationSettings settings);
}

internal sealed class SimulationContext : ISimulationContext
{
    public SimulationPayload Payload { get; private set; }

    public void SetSettings(SimulationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        Payload = new SimulationPayload(settings);
    }
}
