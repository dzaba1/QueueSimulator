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
    private readonly Dictionary<long, Agent> agentsCache = new Dictionary<long, Agent>();

    public AgentsQueue(SimulationPayload simulationPayload)
    {
        ArgumentNullException.ThrowIfNull(simulationPayload, nameof(simulationPayload));

        agentConfigurationsCached = simulationPayload.AgentConfigurationsCached;
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
            agentsCache.Add(agent.Id, agent);
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
            .Select(a => new { AgentConfiguration = a.Key, ActiveAgentsCount = ActiveAgentsCount(a.Value) })
            .Where(a => a.ActiveAgentsCount > 0)
            .ToDictionary(a => a.AgentConfiguration, a => a.ActiveAgentsCount, StringComparer.OrdinalIgnoreCase);
    }

    public Agent GetAgent(long id)
    {
        return agentsCache[id];
    }
}
