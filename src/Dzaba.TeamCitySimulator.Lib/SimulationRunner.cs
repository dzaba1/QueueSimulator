using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;

namespace Dzaba.TeamCitySimulator.Lib;

internal sealed class SimulationRunner
{
    private readonly SimulationSettings simulationSettings;
    private readonly DateTime startDate = new DateTime(2025, 1, 1);

    public SimulationRunner(SimulationSettings simulationSettings)
    {
        ArgumentNullException.ThrowIfNull(simulationSettings, nameof(simulationSettings));

        this.simulationSettings = simulationSettings;
    }

    public IEnumerable<EventData> Run()
    {
        throw new NotImplementedException();
    }
}
