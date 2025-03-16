using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib;

internal interface ISimulationEvents : IEnumerable<TimeEventData>
{
    void AddTimedEventData(EventData data, string message);
}

internal sealed class SimulationEvents : ISimulationEvents
{
    private readonly List<TimeEventData> timeEvents = new();
    private readonly ISimulationContext context;
    private readonly IRequestsRepository requestRepo;
    private readonly IAgentsRepository agentsRepo;

    public SimulationEvents(ISimulationContext context,
        IRequestsRepository requestRepo,
        IAgentsRepository agentsRepo)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(requestRepo, nameof(requestRepo));
        ArgumentNullException.ThrowIfNull(agentsRepo, nameof(agentsRepo));

        this.context = context;
        this.requestRepo = requestRepo;
        this.agentsRepo = agentsRepo;
    }

    public void AddTimedEventData(EventData data, string message)
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

        var requestsQueueData = new ElementsData
        {
            Total = requestRepo.GetQueueLength(),
            Grouped = requestRepo.GroupQueueByConfiguration()
                .Select(g => new NamedQueueData
                {
                    Name = g.Key,
                    Length = g.Value.Length,
                })
                .ToArray()
        };

        var runningAgents = new ElementsData
        {
            Total = agentsRepo.GetActiveAgentsCount(),
            Grouped = agentsRepo.GetActiveAgentsByConfigurationCount()
                .Select(g => new NamedQueueData
                {
                    Name = g.Key,
                    Length = g.Value,
                })
                .ToArray()
        };

        var runningRequests = new ElementsData
        {
            Total = requestRepo.GetRunningRequestCount(),
            Grouped = requestRepo.GroupRunningRequestsByConfiguration()
                .Select(g => new NamedQueueData
                {
                    Name = g.Key,
                    Length = g.Value.Length,
                })
                .ToArray()
        };

        var timedEvent = new TimeEventData
        {
            Timestamp = data.Time,
            Name = data.Name,
            Message = message,
            RequestsQueue = requestsQueueData,
            RunningAgents = runningAgents,
            RunningRequests = runningRequests
        };

        if (context.Payload.SimulationSettings.IncludeAllAgents)
        {
            timedEvent.AllAgents = agentsRepo.EnumerateAgents()
                .Select(a => a.ShallowCopy())
                .ToArray();
        }

        if (context.Payload.SimulationSettings.IncludeAllRequests)
        {
            timedEvent.AllRequests = requestRepo.EnumerateRequests()
                .Select(a => a.ShallowCopy())
                .ToArray();
        }

        timeEvents.Add(timedEvent);
    }

    public IEnumerator<TimeEventData> GetEnumerator()
    {
        return ((IEnumerable<TimeEventData>)timeEvents).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)timeEvents).GetEnumerator();
    }
}
