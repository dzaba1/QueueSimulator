using Dzaba.QueueSimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Repositories;

internal interface IRequestsRepository
{
    IEnumerable<Request> EnumerateRequests();
    Request GetRequest(long id);
    int GetQueueLength();
    int GetRunningRequestCount();
    IEnumerable<Request> GetWaitingForAgents();
    IEnumerable<Request> GetWaitingForDependencies();
    IReadOnlyDictionary<string, Request[]> GroupQueueByConfiguration();
    IReadOnlyDictionary<string, Request[]> GroupRunningRequestsByConfiguration();
    Request NewRequest(RequestConfiguration requestConfiguration, DateTime currentTime);
    IEnumerable<RequestConfiguration> ResolveRequestConfigurationDependencies(RequestConfiguration requestConfiguration, bool recursive);
}

internal sealed class RequestsRepository : IRequestsRepository
{
    private readonly ISimulationContext simulationContext;
    private readonly LongSequence idSequence = new();
    private readonly Dictionary<long, Request> allRequests = new();

    public RequestsRepository(ISimulationContext simulationContext)
    {
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));

        this.simulationContext = simulationContext;
    }

    public Request NewRequest(RequestConfiguration requestConfiguration, DateTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(requestConfiguration, nameof(requestConfiguration));

        var request = new Request
        {
            Id = idSequence.Next(),
            RequestConfiguration = requestConfiguration.Name,
            CreatedTime = currentTime,
        };
        allRequests.Add(request.Id, request);

        return request;
    }

    public IReadOnlyDictionary<string, Request[]> GroupQueueByConfiguration()
    {
        return EnumerateRequests()
            .Where(IsQueued)
            .GroupByToArrayDict(b => b.RequestConfiguration, StringComparer.OrdinalIgnoreCase);
    }

    private bool IsQueued(Request request)
    {
        return request.State != RequestState.Running && request.State != RequestState.Finished;
    }

    public int GetQueueLength()
    {
        return EnumerateRequests().Count(IsQueued);
    }

    public int GetRunningRequestCount()
    {
        return EnumerateRequests().Count(b => b.State == RequestState.Running);
    }

    public IReadOnlyDictionary<string, Request[]> GroupRunningRequestsByConfiguration()
    {
        return EnumerateRequests()
            .Where(b => b.State == RequestState.Running)
            .GroupByToArrayDict(b => b.RequestConfiguration, StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<Request> GetWaitingForAgents()
    {
        return EnumerateRequests()
            .Where(b => b.State == RequestState.WaitingForAgent)
            .Where(b => b.AgentId == null);
    }

    public IEnumerable<Request> GetWaitingForDependencies()
    {
        return EnumerateRequests()
            .Where(b => b.State == RequestState.WaitingForDependencies);
    }

    public IEnumerable<Request> EnumerateRequests()
    {
        return allRequests.Values;
    }

    public Request GetRequest(long id)
    {
        return allRequests[id];
    }

    public IEnumerable<RequestConfiguration> ResolveRequestConfigurationDependencies(RequestConfiguration requestConfiguration, bool recursive)
    {
        ArgumentNullException.ThrowIfNull(requestConfiguration, nameof(requestConfiguration));

        var comparer = new StringPropertyEqualityComparer<RequestConfiguration>(r => r.Name, StringComparer.OrdinalIgnoreCase);

        return ResolveRequestConfigurationDependenciesInternal(requestConfiguration, recursive)
            .Distinct(comparer);
    }

    private IEnumerable<RequestConfiguration> ResolveRequestConfigurationDependenciesInternal(RequestConfiguration requestConfiguration, bool recursive)
    {
        if (requestConfiguration.RequestDependencies == null)
        {
            yield break;
        }

        var current = requestConfiguration.RequestDependencies
                .Select(simulationContext.Payload.GetRequestConfiguration);

        foreach (var dep in current)
        {
            yield return dep;

            if (recursive)
            {
                var subDeps = ResolveRequestConfigurationDependenciesInternal(dep, true);
                foreach (var subDep in subDeps)
                {
                    yield return subDep;
                }
            }
        }
    }
}
