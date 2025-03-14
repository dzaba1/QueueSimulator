using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;

namespace Dzaba.TeamCitySimulator.Lib;

internal interface ISimulationValidation
{
    void Validate(IReadOnlyDictionary<string, Build> buildsCached,
        IReadOnlyDictionary<string, Agent> agentsCached,
        IEnumerable<QueuedBuild> queuedBuilds);
}

internal sealed class SimulationValidation : ISimulationValidation
{
    public void Validate(IReadOnlyDictionary<string, Build> buildsCached,
        IReadOnlyDictionary<string, Agent> agentsCached,
        IEnumerable<QueuedBuild> queuedBuilds)
    {
        ArgumentNullException.ThrowIfNull(buildsCached, nameof(buildsCached));
        ArgumentNullException.ThrowIfNull(agentsCached, nameof(agentsCached));

        foreach (var build in buildsCached.Values)
        {
            foreach (var agentName in build.CompatibleAgents)
            {
                if (!agentsCached.ContainsKey(agentName))
                {
                    throw new ExitCodeException(ExitCode.BuildAgentNotFound, $"Couldn't find agent {agentName} for build configuration {build.Name}.");
                }
            }

            if (build.BuildDependencies != null)
            {
                foreach (var buildName in build.BuildDependencies)
                {
                    if (!buildsCached.ContainsKey(buildName))
                    {
                        throw new ExitCodeException(ExitCode.BuildNotFound, $"Couldn't find dependent build configuration {buildName} for build configuration {build.Name}.");
                    }
                }
            }
        }

        foreach (var queuedBuild in queuedBuilds)
        {
            if (!buildsCached.TryGetValue(queuedBuild.Name, out var build))
            {
                throw new ExitCodeException(ExitCode.BuildNotFound, $"Couldn't find build configuration {queuedBuild.Name}.");
            }

            HashSet<string> checkedBuilds = new HashSet<string>();
            checkedBuilds.Add(queuedBuild.Name);
        }
    }
}
