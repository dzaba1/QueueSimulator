using Dzaba.QueueSimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Repositories;

internal interface IAgentsRepository
{
    IEnumerable<Agent> EnumerateAgents();
    IReadOnlyDictionary<string, int> GetActiveAgentsByConfigurationCount();
    int GetActiveAgentsCount();
    Agent GetAgent(long id);
    bool TryCreateAgent(IEnumerable<string> compatibleAgents, DateTime currentTime, out Agent agent);
}

internal sealed class AgentsRepository : IAgentsRepository
{
    private readonly LongSequence agentIdSequence = new();
    private readonly Lazy<Dictionary<string, List<Agent>>> agentsConfigurationIndex;
    private readonly Dictionary<long, Agent> allAgents = new Dictionary<long, Agent>();
    private readonly ISimulationContext simulationContext;

    public AgentsRepository(ISimulationContext simulationContext)
    {
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));

        this.simulationContext = simulationContext;

        agentsConfigurationIndex = new Lazy<Dictionary<string, List<Agent>>>(InitAgentsConfigurationIndex);
    }

    private Dictionary<string, List<Agent>> InitAgentsConfigurationIndex()
    {
        var local = new Dictionary<string, List<Agent>>(StringComparer.OrdinalIgnoreCase);
        foreach (var agentConfiguration in simulationContext.Payload.AgentConfigurationsCached)
        {
            local.Add(agentConfiguration.Key, new List<Agent>());
        }
        return local;
    }

    public bool TryCreateAgent(IEnumerable<string> compatibleAgents, DateTime currentTime, out Agent agent)
    {
        ArgumentNullException.ThrowIfNull(compatibleAgents, nameof(compatibleAgents));

        var simulationPayload = simulationContext.Payload;

        if (simulationPayload.SimulationSettings.MaxRunningAgents != null && GetActiveAgentsCount() == simulationPayload.SimulationSettings.MaxRunningAgents.Value)
        {
            agent = null;
            return false;
        }

        var tempSet = new HashSet<string>(compatibleAgents, StringComparer.OrdinalIgnoreCase);

        var ordered = agentsConfigurationIndex.Value
            .Where(a => tempSet.Contains(a.Key))
            .Select(a => new
            {
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
        return agentsConfigurationIndex.Value
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
