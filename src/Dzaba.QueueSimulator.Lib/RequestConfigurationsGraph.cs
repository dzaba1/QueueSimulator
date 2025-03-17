using Dzaba.QueueSimulator.Lib.Model;
using System;
using System.Collections.Generic;

namespace Dzaba.QueueSimulator.Lib;

internal sealed class RequestConfigurationsGraph
{
    private readonly IEqualityComparer<RequestConfiguration> requestConfigurationComparer;
    private readonly SimulationPayload simulationPayload;
    private readonly Dictionary<RequestConfiguration, List<RequestConfiguration>> children;
    private readonly Dictionary<RequestConfiguration, HashSet<RequestConfiguration>> parents;

    public RequestConfigurationsGraph(SimulationPayload simulationPayload,
        RequestConfiguration initRequestConfiguration)
    {
        ArgumentNullException.ThrowIfNull(simulationPayload, nameof(simulationPayload));
        ArgumentNullException.ThrowIfNull(initRequestConfiguration, nameof(initRequestConfiguration));

        this.simulationPayload = simulationPayload;

        requestConfigurationComparer = new StringPropertyEqualityComparer<RequestConfiguration>(c => c.Name, StringComparer.OrdinalIgnoreCase);
        children = new Dictionary<RequestConfiguration, List<RequestConfiguration>>(requestConfigurationComparer);
        parents = new Dictionary<RequestConfiguration, HashSet<RequestConfiguration>>(requestConfigurationComparer);

        Add(initRequestConfiguration, null);
    }

    private void Add(RequestConfiguration requestConfiguration, RequestConfiguration parent)
    {
        if (!parents.TryGetValue(requestConfiguration, out var parentsList))
        {
            parentsList = new HashSet<RequestConfiguration>(requestConfigurationComparer);
            parents.Add(requestConfiguration, parentsList);
        }

        if (parent != null)
        {
            parentsList.Add(parent);
        }

        if (!children.TryGetValue(requestConfiguration, out var childrenlist))
        {
            childrenlist = new List<RequestConfiguration>();
            children.Add(requestConfiguration, childrenlist);

            foreach (var dep in requestConfiguration.ResolveDependencies(simulationPayload, false))
            {
                childrenlist.Add(dep);
                Add(dep, requestConfiguration);
            }
        }
    }

    public IEnumerable<RequestConfiguration> GetChildren(RequestConfiguration requestConfiguration)
    {
        ArgumentNullException.ThrowIfNull(requestConfiguration, nameof(requestConfiguration));

        return children[requestConfiguration];
    }

    public IEnumerable<RequestConfiguration> GetParents(RequestConfiguration requestConfiguration)
    {
        ArgumentNullException.ThrowIfNull(requestConfiguration, nameof(requestConfiguration));

        return parents[requestConfiguration];
    }
}
