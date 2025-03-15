using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib;

internal interface ISimulationValidation
{
    void Validate(IReadOnlyDictionary<string, BuildConfiguration> buildsCached,
        IReadOnlyDictionary<string, Agent> agentsCached,
        IEnumerable<QueuedBuild> queuedBuilds);
}

internal sealed class SimulationValidation : ISimulationValidation
{
    public void Validate(IReadOnlyDictionary<string, BuildConfiguration> buildsCached,
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
            if (!buildsCached.ContainsKey(queuedBuild.Name))
            {
                throw new ExitCodeException(ExitCode.BuildNotFound, $"Couldn't find build configuration {queuedBuild.Name}.");
            }

            Stack<CyclicChain> toCheck = new Stack<CyclicChain>();
            toCheck.Push(new CyclicChain(queuedBuild.Name, []));
            
            while (toCheck.Count > 0)
            {
                var buildChain = toCheck.Pop();
                
                var chainSet = new HashSet<string>(buildChain.Chain, StringComparer.OrdinalIgnoreCase);
                if (chainSet.Contains(buildChain.BuildName))
                {
                    throw new ExitCodeException(ExitCode.BuildCyclicDependency, $"Detected build cyclic dependnecy on {buildChain.BuildName} starting from {queuedBuild.Name}.");
                }
       
                chainSet.Add(buildChain.BuildName);
                var build = buildsCached[buildChain.BuildName];
                if (build.BuildDependencies != null)
                {
                    foreach (var buildDep in build.BuildDependencies)
                    {
                        toCheck.Push(new CyclicChain(buildDep, chainSet));
                    }
                }
            }
        }
    }

    private class CyclicChain
    {
        public CyclicChain(string buildName, IEnumerable<string> chain)
        {
            BuildName = buildName;
            Chain = chain.ToArray();
        }

        public string BuildName { get; }
        public string[] Chain { get; }
    }
}
