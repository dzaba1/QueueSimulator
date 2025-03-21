using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
using System;
using System.Collections.Generic;

namespace Dzaba.QueueSimulator.Lib;

public interface ISimulation
{
    IEnumerable<TimeEventData> Run(SimulationSettings settings);
}

internal sealed class Simulation : ISimulation
{
    public static readonly DateTimeOffset StartTime = new DateTime(2025, 1, 1).ToUniversalTime().Date;

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

        InitRequests();

        eventsQueue.Run();
        return simulationEvents;
    }

    private void InitRequests()
    {
        var simulationPayload = context.Payload;

        foreach (var initRequest in simulationPayload.SimulationSettings.InitialRequests)
        {
            var waitTime = simulationPayload.SimulationSettings.SimulationDuration / initRequest.NumberToQueue;
            for (var i = 0; i < initRequest.NumberToQueue; i++)
            {
                var requestStartTime = StartTime + waitTime * i;
                var request = simulationPayload.GetRequestConfiguration(initRequest.Name);
                var eventPayload = new QueueRequestEventPayload(request, new Pipeline(request, simulationPayload), null);

                eventsQueue.AddQueueRequestQueueEvent(eventPayload, requestStartTime);
            }
        }
    }
}
