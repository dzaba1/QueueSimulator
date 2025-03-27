using Dzaba.QueueSimulator.Lib.Model;
using Microsoft.Extensions.Logging;
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
    bool TryCreateAgent(IEnumerable<string> compatibleAgents, DateTimeOffset currentTime, out Agent agent);
    bool MaxAgentsReached();
    bool CanAgentBeCreated(IEnumerable<string> compatibleAgents);
}

internal sealed class AgentsRepository : IAgentsRepository
{
    private readonly LongSequence agentIdSequence = new();
    private readonly Lazy<Dictionary<string, List<Agent>>> agentsConfigurationIndex;
    private readonly Dictionary<long, Agent> allAgents = new Dictionary<long, Agent>();
    private readonly ISimulationContext simulationContext;
    private readonly ILogger<AgentsRepository> logger;

    public AgentsRepository(ISimulationContext simulationContext,
        ILogger<AgentsRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        this.simulationContext = simulationContext;
        this.logger = logger;

        agentsConfigurationIndex = new Lazy<Dictionary<string, List<Agent>>>(InitAgentsConfigurationIndex);
    }

    private Dictionary<string, List<Agent>> InitAgentsConfigurationIndex()
    {
        var local = new Dictionary<string, List<Agent>>(StringComparer.OrdinalIgnoreCase);
        foreach (var agentConfiguration in simulationContext.Payload.AgentConfigurations.Cache)
        {
            local.Add(agentConfiguration.Key, new List<Agent>());
        }
        return local;
    }

    public bool TryCreateAgent(IEnumerable<string> compatibleAgents, DateTimeOffset currentTime, out Agent agent)
    {
        ArgumentNullException.ThrowIfNull(compatibleAgents, nameof(compatibleAgents));

        var simulationPayload = simulationContext.Payload;

        if (MaxAgentsReached())
        {
            logger.LogDebug("Max total active agents reached: {MaxRunningAgents}", simulationPayload.SimulationSettings.MaxRunningAgents.Value);
            agent = null;
            return false;
        }

        var list = GetAgentList(compatibleAgents);

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

            logger.LogDebug("Created a new agent with ID {AgentId} from configuration {Agent}.", agent.Id, agent.AgentConfiguration);

            return true;
        }

        agent = null;
        return false;
    }

    public int GetActiveAgentsCount()
    {
        return GetActiveAgentsCount(EnumerateAgents(), false);
    }

    private int GetActiveAgentsCount(IEnumerable<Agent> agents, bool includeExternal)
    {
        var all = agents
            .Select(a => new { Agent = a, Configuration = simulationContext.Payload.AgentConfigurations.GetEntity(a.AgentConfiguration) });

        if (!includeExternal)
        {
            all = all.Where(a => !a.Configuration.IsExternal);
        }

        return all.Count(a => a.Agent.State != AgentState.Finished);
    }

    public IReadOnlyDictionary<string, int> GetActiveAgentsByConfigurationCount()
    {
        return agentsConfigurationIndex.Value
            .Select(a => new { AgentConfiguration = a.Key, ActiveAgentsCount = GetActiveAgentsCount(a.Value, true) })
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

    public bool MaxAgentsReached()
    {
        var maxAgents = simulationContext.Payload.SimulationSettings.MaxRunningAgents;
        return maxAgents != null && GetActiveAgentsCount() == maxAgents.Value;
    }

    private AgentList GetAgentList(IEnumerable<string> compatibleAgents)
    {
        var tempSet = new HashSet<string>(compatibleAgents, StringComparer.OrdinalIgnoreCase);

        var ordered = agentsConfigurationIndex.Value
            .Where(a => tempSet.Contains(a.Key))
            .Select(a => new AgentList
            {
                AgentConfiguration = simulationContext.Payload.AgentConfigurations.GetEntity(a.Key),
                Agents = a.Value,
                ActiveAgentsCount = GetActiveAgentsCount(a.Value, true)
            })
            .OrderBy(a => a.ActiveAgentsCount);

        return ordered.FirstOrDefault(a =>
        {
            if (a.AgentConfiguration.MaxInstances != null)
            {
                return a.ActiveAgentsCount < a.AgentConfiguration.MaxInstances.Value;
            }
            return true;
        });
    }

    public bool CanAgentBeCreated(IEnumerable<string> compatibleAgents)
    {
        ArgumentNullException.ThrowIfNull(compatibleAgents, nameof(compatibleAgents));

        if (MaxAgentsReached())
        {
            return false;
        }

        return GetAgentList(compatibleAgents) != null;
    }

    private class AgentList
    {
        public AgentConfiguration AgentConfiguration { get; init; }
        public int ActiveAgentsCount { get; init; }
        public List<Agent> Agents { get; init; }
    }
}
