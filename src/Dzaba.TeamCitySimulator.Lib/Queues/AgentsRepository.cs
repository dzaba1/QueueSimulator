using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib.Queues;

internal sealed class AgentsRepository
{
    private readonly SimulationPayload simulationPayload;
    private readonly LongSequence agentIdSequence = new();
    private readonly Dictionary<string, List<Agent>> agentsConfigurationIndex = new Dictionary<string, List<Agent>>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<long, Agent> allAgents = new Dictionary<long, Agent>();

    public AgentsRepository(SimulationPayload simulationPayload)
    {
        ArgumentNullException.ThrowIfNull(simulationPayload, nameof(simulationPayload));

        this.simulationPayload = simulationPayload;
        foreach (var agentConfiguration in simulationPayload.AgentConfigurationsCached)
        {
            agentsConfigurationIndex.Add(agentConfiguration.Key, new List<Agent>());
        }
    }

    public bool TryInitAgent(IEnumerable<string> compatibleAgents, DateTime currentTime, out Agent agent)
    {
        ArgumentNullException.ThrowIfNull(compatibleAgents, nameof(compatibleAgents));

        if (simulationPayload.SimulationSettings.MaxRunningAgents != null && GetActiveAgentsCount() == simulationPayload.SimulationSettings.MaxRunningAgents.Value)
        {
            agent = null;
            return false;
        }

        var tempSet = new HashSet<string>(compatibleAgents, StringComparer.OrdinalIgnoreCase);

        var ordered = agentsConfigurationIndex
            .Where(a => tempSet.Contains(a.Key))
            .Select(a => new {
                AgentConfiguration = simulationPayload.GetAgentConfiguration(a.Key),
                Agents = a.Value,
                ActiveAgentsCount = GetActiveAgentsCount(a.Value)
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
            allAgents.Add(agent.Id, agent);
            return true;
        }

        agent = null;
        return false;
    }

    public int GetActiveAgentsCount()
    {
        return GetActiveAgentsCount(EnumerateAgents());
    }

    private int GetActiveAgentsCount(IEnumerable<Agent> agents)
    {
        return agents.Count(a => a.State != AgentState.Finished);
    }

    public IReadOnlyDictionary<string, int> GetActiveAgentsByConfigurationCount()
    {
        return agentsConfigurationIndex
            .Select(a => new { AgentConfiguration = a.Key, ActiveAgentsCount = GetActiveAgentsCount(a.Value) })
            .Where(a => a.ActiveAgentsCount > 0)
            .ToDictionary(a => a.AgentConfiguration, a => a.ActiveAgentsCount, StringComparer.OrdinalIgnoreCase);
    }

    public Agent GetAgent(long id)
    {
        return allAgents[id];
    }

    public IEnumerable<Agent> EnumerateAgents()
    {
        return allAgents.Values;
    }
}
