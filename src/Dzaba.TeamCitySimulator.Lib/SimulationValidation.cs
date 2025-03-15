using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib;

internal interface ISimulationValidation
{
    void Validate(SimulationPayload simulationPayload);
}

internal sealed class SimulationValidation : ISimulationValidation
{
    public void Validate(SimulationPayload simulationPayload)
    {
        ArgumentNullException.ThrowIfNull(simulationPayload, nameof(simulationPayload));

        foreach (var build in simulationPayload.BuildConfigurationsCached.Values)
        {
            foreach (var agentName in build.CompatibleAgents)
            {
                if (!simulationPayload.AgentConfigurationsCached.ContainsKey(agentName))
                {
                    throw new ExitCodeException(ExitCode.BuildAgentNotFound, $"Couldn't find agent {agentName} for build configuration {build.Name}.");
                }
            }

            if (build.BuildDependencies != null)
            {
                foreach (var buildName in build.BuildDependencies)
                {
                    if (!simulationPayload.BuildConfigurationsCached.ContainsKey(buildName))
                    {
                        throw new ExitCodeException(ExitCode.BuildNotFound, $"Couldn't find dependent build configuration {buildName} for build configuration {build.Name}.");
                    }
                }
            }
        }

        foreach (var queuedBuild in simulationPayload.SimulationSettings.QueuedBuilds)
        {
            if (!simulationPayload.BuildConfigurationsCached.ContainsKey(queuedBuild.Name))
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
                var build = simulationPayload.GetBuildConfiguration(buildChain.BuildName);
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
