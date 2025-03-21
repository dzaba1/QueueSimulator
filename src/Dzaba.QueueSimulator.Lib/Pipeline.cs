using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib;

internal interface IPipeline
{
    RequestConfiguration InitRequestConfiguration { get; }
    RequestConfigurationsGraph RequestConfigurationsGraph { get; }

    IEnumerable<Request> GetChildren(Request request);
    IEnumerable<Request> GetParents(Request request, bool recursive);
    void SetReference(Request currentRequest, Request parent);
    void SetRequest(RequestConfiguration requestConfiguration, Request request);
    bool TryGetRequest(RequestConfiguration requestConfiguration, out Request request);
}

internal sealed class Pipeline : IPipeline
{
    private readonly OnePropertyComparer<Request, long> requestIdComparer;
    private readonly Dictionary<Request, HashSet<Request>> requestParents;
    private readonly Dictionary<Request, HashSet<Request>> requestChildren;
    private readonly Dictionary<RequestConfiguration, Request> createdRequests;

    public Pipeline(RequestConfiguration initRequestConfiguration,
        SimulationPayload simulationPayload)
    {
        ArgumentNullException.ThrowIfNull(initRequestConfiguration, nameof(initRequestConfiguration));
        ArgumentNullException.ThrowIfNull(simulationPayload, nameof(simulationPayload));

        InitRequestConfiguration = initRequestConfiguration;

        requestIdComparer = new OnePropertyComparer<Request, long>(r => r.Id);
        RequestConfigurationsGraph = new RequestConfigurationsGraph(simulationPayload, initRequestConfiguration);

        requestParents = new Dictionary<Request, HashSet<Request>>(requestIdComparer);
        requestChildren = new Dictionary<Request, HashSet<Request>>(requestIdComparer);
        createdRequests = new Dictionary<RequestConfiguration, Request>(new StringPropertyEqualityComparer<RequestConfiguration>(r => r.Name, StringComparer.OrdinalIgnoreCase));
    }

    public RequestConfiguration InitRequestConfiguration { get; }

    public RequestConfigurationsGraph RequestConfigurationsGraph { get; }

    public void SetReference(Request currentRequest, Request parent)
    {
        ArgumentNullException.ThrowIfNull(currentRequest, nameof(currentRequest));
        ArgumentNullException.ThrowIfNull(parent, nameof(parent));

        if (!requestParents.TryGetValue(currentRequest, out var parentList))
        {
            parentList = new HashSet<Request>(requestIdComparer);
            requestParents.Add(currentRequest, parentList);
        }

        if (!requestChildren.TryGetValue(parent, out var childrenList))
        {
            childrenList = new HashSet<Request>(requestIdComparer);
            requestChildren.Add(parent, childrenList);
        }

        parentList.Add(parent);
        childrenList.Add(currentRequest);
    }

    public bool TryGetRequest(RequestConfiguration requestConfiguration, out Request request)
    {
        ArgumentNullException.ThrowIfNull(requestConfiguration, nameof(requestConfiguration));

        return createdRequests.TryGetValue(requestConfiguration, out request);
    }

    public void SetRequest(RequestConfiguration requestConfiguration, Request request)
    {
        ArgumentNullException.ThrowIfNull(requestConfiguration, nameof(requestConfiguration));
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        createdRequests.Add(requestConfiguration, request);
    }

    public IEnumerable<Request> GetChildren(Request request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (requestChildren.TryGetValue(request, out var list))
        {
            return list;
        }
        return Enumerable.Empty<Request>();
    }

    public IEnumerable<Request> GetParents(Request request, bool recursive)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (!requestParents.TryGetValue(request, out var list))
        {
            yield break;
        }

        foreach (var item in list)
        {
            yield return item;

            if (recursive)
            {
                var nextParents = GetParents(item, true);
                foreach (var nextItem in nextParents)
                {
                    yield return nextItem;
                }
            }
        }
    }
}
