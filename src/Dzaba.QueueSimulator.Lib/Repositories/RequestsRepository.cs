using Dzaba.QueueSimulator.Lib.Model;
using Microsoft.Extensions.Logging;
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
    Request NewRequest(RequestConfiguration requestConfiguration, IPipeline pipeline, DateTime currentTime);
    IPipeline GetPipeline(Request request);
}

internal sealed class RequestsRepository : IRequestsRepository
{
    private readonly LongSequence idSequence = new();
    private readonly Dictionary<long, Request> allRequests = new();
    private readonly Dictionary<long, IPipeline> allPipelines = new();
    private readonly ILogger<RequestsRepository> logger;

    public RequestsRepository(ILogger<RequestsRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        this.logger = logger;
    }

    public Request NewRequest(RequestConfiguration requestConfiguration,IPipeline pipeline, DateTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(requestConfiguration, nameof(requestConfiguration));
        ArgumentNullException.ThrowIfNull(pipeline, nameof(pipeline));

        var request = new Request
        {
            Id = idSequence.Next(),
            RequestConfiguration = requestConfiguration.Name,
            CreatedTime = currentTime,
        };
        allRequests.Add(request.Id, request);
        allPipelines.Add(request.Id, pipeline);
        pipeline.SetRequest(requestConfiguration, request);

        logger.LogDebug("Created a new requst with ID {RequestId} from configuration {Request}.", request.Id, requestConfiguration.Name);

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
            .Where(b => b.State == RequestState.WaitingForAgent);
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

    public IPipeline GetPipeline(Request request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        return allPipelines[request.Id];
    }
}
