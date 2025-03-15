using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;

namespace Dzaba.TeamCitySimulator.Lib;

public interface ISimulation
{
    IEnumerable<TimeEventData> Run(SimulationSettings settings);
}

internal sealed class Simulation : ISimulation
{
    private readonly ISimulationValidation simulationValidation;

    public Simulation(ISimulationValidation simulationValidation)
    {
        ArgumentNullException.ThrowIfNull(simulationValidation, nameof(simulationValidation));

        this.simulationValidation = simulationValidation;
    }

    public IEnumerable<TimeEventData> Run(SimulationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        
        var runner = new SimulationRunner(settings, simulationValidation);
        return runner.Run();
    }
}
