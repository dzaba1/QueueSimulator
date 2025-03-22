using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Dzaba.QueueSimulator.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib;

public interface ISimulation
{
    SimulationReport Run(SimulationSettings settings);
}

internal sealed class Simulation : ISimulation
{
    public static readonly DateTime StartTime = new DateTime(2025, 1, 1);

    private readonly ISimulationValidation simulationValidation;
    private readonly ISimulationContext context;
    private readonly ISimulationEventQueue eventsQueue;
    private readonly ISimulationEvents simulationEvents;
    private readonly IRequestsRepository requestsRepo;
    private readonly IRand rand;

    public Simulation(ISimulationValidation simulationValidation,
        ISimulationContext context,
        ISimulationEventQueue eventsQueue,
        ISimulationEvents simulationEvents,
        IRequestsRepository requestsRepo,
        IRand rand)
    {
        ArgumentNullException.ThrowIfNull(simulationValidation, nameof(simulationValidation));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(eventsQueue, nameof(eventsQueue));
        ArgumentNullException.ThrowIfNull(simulationEvents, nameof(simulationEvents));
        ArgumentNullException.ThrowIfNull(requestsRepo, nameof(requestsRepo));
        ArgumentNullException.ThrowIfNull(rand, nameof(rand));

        this.simulationValidation = simulationValidation;
        this.context = context;
        this.eventsQueue = eventsQueue;
        this.simulationEvents = simulationEvents;
        this.requestsRepo = requestsRepo;
        this.rand = rand;
    }

    public SimulationReport Run(SimulationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        context.SetSettings(settings);

        simulationValidation.Validate(context.Payload);

        InitRequests();

        eventsQueue.Run();

        return new SimulationReport
        {
            Events = simulationEvents.ToArray(),
            RequestDurationStatistics = GetRequestDurationStats().ToArray()
        };
    }

    private IEnumerable<RequestConfigurationStatistics> GetRequestDurationStats()
    {
        return requestsRepo.EnumerateRequests()
            .Where(r => context.Payload.ShouldObserveRequest(r.RequestConfiguration))
            .Where(r => r.State == RequestState.Finished)
            .GroupBy(r => r.RequestConfiguration, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var allItems = g.ToArray();

                return new RequestConfigurationStatistics
                {
                    Name = g.Key,
                    AvgDuration = allItems.Average(r => r.RunningDuration().Value),
                    MaxDuration = allItems.Max(r => r.RunningDuration().Value),
                    MinDuration = allItems.Min(r => r.RunningDuration().Value),
                    AvgQueueTime = allItems.Average(r => r.QueueDuration().Value),
                    MaxQueueTime = allItems.Max(r => r.QueueDuration().Value),
                    MinQueueTime = allItems.Min(r => r.QueueDuration().Value),
                    AvgTotalDuration = allItems.Average(r => r.TotalDuration().Value),
                    MaxTotalDuration = allItems.Max(r => r.TotalDuration().Value),
                    MinTotalDuration = allItems.Min(r => r.TotalDuration().Value)
                };
            });
    }

    private void InitRequests()
    {
        var simulationPayload = context.Payload;

        foreach (var initRequest in simulationPayload.SimulationSettings.InitialRequests)
        {
            var initTimes = initRequest.Distribution.GetInitTimes(StartTime, rand);
            foreach (var initTime in initTimes)
            {
                var request = simulationPayload.GetRequestConfiguration(initRequest.Name);
                var eventPayload = new QueueRequestEventPayload(request, new Pipeline(request, simulationPayload), null);
                eventsQueue.AddQueueRequestQueueEvent(eventPayload, initTime);
            }
        }
    }
}
