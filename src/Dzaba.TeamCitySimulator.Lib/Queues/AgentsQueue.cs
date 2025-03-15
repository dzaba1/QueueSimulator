using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib.Queues;

internal sealed class AgentsQueue
{
    private readonly IReadOnlyDictionary<string, AgentConfiguration> agentConfigurationsCached;
    private readonly LongSequence agentIdSequence = new();
    private readonly Dictionary<string, List<Agent>> allAgents = new Dictionary<string, List<Agent>>(StringComparer.OrdinalIgnoreCase);

    public AgentsQueue(IReadOnlyDictionary<string, AgentConfiguration> agentConfigurationsCached)
    {
        ArgumentNullException.ThrowIfNull(agentConfigurationsCached, nameof(agentConfigurationsCached));

        this.agentConfigurationsCached = agentConfigurationsCached;
        foreach (var agentConfiguration in agentConfigurationsCached)
        {
            allAgents.Add(agentConfiguration.Key, new List<Agent>());
        }
    }

    public bool TryInitAgent(IEnumerable<string> compatibleAgents, DateTime currentTime, out Agent agent)
    {
        ArgumentNullException.ThrowIfNull(compatibleAgents, nameof(compatibleAgents));

        var tempSet = new HashSet<string>(compatibleAgents, StringComparer.OrdinalIgnoreCase);

        var ordered = allAgents
            .Where(a => tempSet.Contains(a.Key))
            .Select(a => new {
                AgentConfiguration = agentConfigurationsCached[a.Key],
                Agents = a.Value,
                ActiveAgentsCount = ActiveAgentsCount(a.Value)
            })
            .OrderBy(a => a.ActiveAgentsCount);

        var list = ordered.FirstOrDefault(a =>
        {
            if (a.AgentConfiguration.MaxInstances != null)
            {
                return a.ActiveAgentsCount < a.AgentConfiguration.MaxInstances.Value;
            }
            return true;
        });

        if (list != null)
        {
            agent = new Agent
            {
                Id = agentIdSequence.Next(),
                CreatedTime = currentTime,
                AgentConfiguration = list.AgentConfiguration.Name
            };
            list.Agents.Add(agent);
            return true;
        }

        agent = null;
        return false;
    }

    private int ActiveAgentsCount(IEnumerable<Agent> agents)
    {
        return agents.Count(a => a.State != AgentState.Finished);
    }

    public IReadOnlyDictionary<string, int> GetActiveAgentsCount()
    {
        return allAgents
            .ToDictionary(a => a.Key, a => ActiveAgentsCount(a.Value), StringComparer.OrdinalIgnoreCase);
    }

    public Agent GetAgent(long id)
    {
        // TODO: cache those
        return allAgents
            .SelectMany(a => a.Value)
            .First(a => a.Id == id);
    }
}
