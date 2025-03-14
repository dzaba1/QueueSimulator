using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;

namespace Dzaba.TeamCitySimulator.Lib;

public interface ISimulation
{
    IEnumerable<EventData> Run(SimulationSettings settings);
}

internal sealed class Simulation : ISimulation
{
    public IEnumerable<EventData> Run(SimulationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        throw new NotImplementedException();
    }
}
