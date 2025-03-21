using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
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

    public Simulation(ISimulationValidation simulationValidation,
        ISimulationContext context,
        ISimulationEventQueue eventsQueue,
        ISimulationEvents simulationEvents,
        IRequestsRepository requestsRepo)
    {
        ArgumentNullException.ThrowIfNull(simulationValidation, nameof(simulationValidation));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(eventsQueue, nameof(eventsQueue));
        ArgumentNullException.ThrowIfNull(simulationEvents, nameof(simulationEvents));
        ArgumentNullException.ThrowIfNull(requestsRepo, nameof(requestsRepo));

        this.simulationValidation = simulationValidation;
        this.context = context;
        this.eventsQueue = eventsQueue;
        this.simulationEvents = simulationEvents;
        this.requestsRepo = requestsRepo;
    }

    public SimulationReport Run(SimulationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        context.SetSettings(settings);

        simulationValidation.Validate(context.Payload);

        InitRequests();

        eventsQueue.Run();

        var report = new SimulationReport
        {
            Events = simulationEvents.ToArray()
        };
        SetAvgValues(report);
        return report;
    }

    private void SetAvgValues(SimulationReport report)
    {
        var requests = requestsRepo.EnumerateRequests()
            .Where(r => r.State == RequestState.Finished)
            .GroupBy(r => r.RequestConfiguration, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var avg = requests.Select(g => new NamedQueueData<TimeSpan>
        {
            Name = g.Key,
            Value = g.Average(r => r.RunningDuration().Value)
        }).ToArray();

        var min = requests.Select(g => new NamedQueueData<TimeSpan>
        {
            Name = g.Key,
            Value = g.Min(r => r.RunningDuration().Value)
        }).ToArray();

        var max = requests.Select(g => new NamedQueueData<TimeSpan>
        {
            Name = g.Key,
            Value = g.Max(r => r.RunningDuration().Value)
        }).ToArray();

        report.RequestDurationStatistics = new RequestDurationStatistics
        {
            AvgRequestDuration = new ElementsData<TimeSpan>
            {
                Grouped = avg,
                Total = avg.Average(g => g.Value)
            },
            MaxRequestDuration = new ElementsData<TimeSpan>
            {
                Grouped = max,
                Total = max.Average(g => g.Value)
            },
            MinRequestDuration = new ElementsData<TimeSpan>
            {
                Grouped = min,
                Total = min.Average(g => g.Value)
            },
        };
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
