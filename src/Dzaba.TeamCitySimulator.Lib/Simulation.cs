using Dzaba.TeamCitySimulator.Lib.Model;
using Microsoft.Extensions.Logging;
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
    private readonly ILoggerFactory loggerFactory;

    public Simulation(ISimulationValidation simulationValidation,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(simulationValidation, nameof(simulationValidation));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));

        this.simulationValidation = simulationValidation;
        this.loggerFactory = loggerFactory;
    }

    public IEnumerable<TimeEventData> Run(SimulationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        
        var runner = new SimulationRunner(settings, simulationValidation, loggerFactory.CreateLogger<SimulationRunner>());
        return runner.Run();
    }
}
