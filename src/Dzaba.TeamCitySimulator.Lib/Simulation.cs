using Dzaba.TeamCitySimulator.Lib.Events;
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
    private static readonly DateTime StartTime = new DateTime(2025, 1, 1);

    private readonly ISimulationValidation simulationValidation;
    private readonly ISimulationContext context;
    private readonly ISimulationEventQueue eventsQueue;
    private readonly ISimulationEvents simulationEvents;

    public Simulation(ISimulationValidation simulationValidation,
        ISimulationContext context,
        ISimulationEventQueue eventsQueue,
        ISimulationEvents simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(simulationValidation, nameof(simulationValidation));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(eventsQueue, nameof(eventsQueue));
        ArgumentNullException.ThrowIfNull(simulationEvents, nameof(simulationEvents));

        this.simulationValidation = simulationValidation;
        this.context = context;
        this.eventsQueue = eventsQueue;
        this.simulationEvents = simulationEvents;
    }

    public IEnumerable<TimeEventData> Run(SimulationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        context.SetSettings(settings);

        simulationValidation.Validate(context.Payload);

        InitBuilds();

        eventsQueue.Run();
        return simulationEvents;
    }

    private void InitBuilds()
    {
        var simulationPayload = context.Payload;

        foreach (var queuedBuild in simulationPayload.SimulationSettings.QueuedBuilds)
        {
            var waitTime = simulationPayload.SimulationSettings.SimulationDuration / queuedBuild.BuildsToQueue;
            for (var i = 0; i < queuedBuild.BuildsToQueue; i++)
            {
                var buildStartTime = StartTime + waitTime * i;
                var build = simulationPayload.GetBuildConfiguration(queuedBuild.Name);
                eventsQueue.AddQueueBuildQueueEvent(build, buildStartTime);
            }
        }
    }
}
