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

        var requestsQueueData = new ElementsData<int>
        {
            Total = requestRepo.GetQueueLength(),
            Grouped = requestRepo.GroupQueueByConfiguration()
                .Where(g => context.Payload.ShouldObserveRequest(g.Key))
                .Select(g => new NamedQueueData<int>
                {
                    Name = g.Key,
                    Value = g.Value.Length,
                })
                .ToArray()
        };

        var runningAgents = new ElementsData<int>
        {
            Total = agentsRepo.GetActiveAgentsCount(),
            Grouped = agentsRepo.GetActiveAgentsByConfigurationCount()
                .Where(g => context.Payload.ShouldObserveAgent(g.Key))
                .Select(g => new NamedQueueData<int>
                {
                    Name = g.Key,
                    Value = g.Value,
                })
                .ToArray()
        };

        var runningRequests = new ElementsData<int>
        {
            Total = requestRepo.GetRunningRequestCount(),
            Grouped = requestRepo.GroupRunningRequestsByConfiguration()
                .Where(g => context.Payload.ShouldObserveRequest(g.Key))
                .Select(g => new NamedQueueData<int>
                {
                    Name = g.Key,
                    Value = g.Value.Length,
                })
                .ToArray()
        };

        var timedEvent = new TimeEventData
        {
            Timestamp = data.Time.DateTime,
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
                .ForEachLazy(GetAndSetDependencies)
                .ToArray();
        }

        timeEvents.Add(timedEvent);
    }

    private void GetAndSetDependencies(Request request)
    {
        var pipeline = requestRepo.GetPipeline(request);
        request.Dependencies = pipeline.GetChildren(request)
            .Select(r => r.Id)
            .ToArray();
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
