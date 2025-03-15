using Dzaba.TeamCitySimulator.Lib.Events;
using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TeamCitySimulator.Lib.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib;

internal interface ISimulationEvents : IEnumerable<TimeEventData>
{
    void AddTimedEventData(EventData data, string message);
}

internal sealed class SimulationEvents : ISimulationEvents
{
    private readonly List<TimeEventData> timeEvents = new();
    private readonly ISimulationContext context;
    private readonly IBuildsRepository buildRepo;
    private readonly IAgentsRepository agentsRepo;

    public SimulationEvents(ISimulationContext context,
        IBuildsRepository buildRepo,
        IAgentsRepository agentsRepo)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(buildRepo, nameof(buildRepo));
        ArgumentNullException.ThrowIfNull(agentsRepo, nameof(agentsRepo));

        this.context = context;
        this.buildRepo = buildRepo;
        this.agentsRepo = agentsRepo;
    }

    public void AddTimedEventData(EventData data, string message)
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));

        var buildsQueueData = new ElementsData
        {
            Total = buildRepo.GetQueueLength(),
            Grouped = buildRepo.GroupQueueByBuildConfiguration()
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

        var runningBuilds = new ElementsData
        {
            Total = buildRepo.GetRunningBuildsCount(),
            Grouped = buildRepo.GroupRunningBuildsByBuildConfiguration()
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
            BuildsQueue = buildsQueueData,
            RunningAgents = runningAgents,
            RunningBuilds = runningBuilds
        };

        if (context.Payload.SimulationSettings.IncludeAllAgents)
        {
            timedEvent.AllAgents = agentsRepo.EnumerateAgents()
                .Select(a => a.ShallowCopy())
                .ToArray();
        }

        if (context.Payload.SimulationSettings.IncludeAllBuilds)
        {
            timedEvent.AllBuilds = buildRepo.EnumerateBuilds()
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
