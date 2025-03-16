using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib;

internal interface ISimulationValidation
{
    void Validate(SimulationPayload simulationPayload);
}

internal sealed class SimulationValidation : ISimulationValidation
{
    public void Validate(SimulationPayload simulationPayload)
    {
        ArgumentNullException.ThrowIfNull(simulationPayload, nameof(simulationPayload));

        foreach (var request in simulationPayload.RequestConfigurationsCached.Values)
        {
            foreach (var agentName in request.CompatibleAgents)
            {
                if (!simulationPayload.AgentConfigurationsCached.ContainsKey(agentName))
                {
                    throw new ExitCodeException(ExitCode.AgentNotFound, $"Couldn't find agent {agentName} for request configuration {request.Name}.");
                }
            }

            if (request.RequestDependencies != null)
            {
                foreach (var requestName in request.RequestDependencies)
                {
                    if (!simulationPayload.RequestConfigurationsCached.ContainsKey(requestName))
                    {
                        throw new ExitCodeException(ExitCode.RequestNotFound, $"Couldn't find dependent request configuration {requestName} for request configuration {request.Name}.");
                    }
                }
            }
        }

        foreach (var queuedRequest in simulationPayload.SimulationSettings.InitialRequests)
        {
            if (!simulationPayload.RequestConfigurationsCached.ContainsKey(queuedRequest.Name))
            {
                throw new ExitCodeException(ExitCode.RequestNotFound, $"Couldn't find request configuration {queuedRequest.Name}.");
            }

            Stack<CyclicChain> toCheck = new Stack<CyclicChain>();
            toCheck.Push(new CyclicChain(queuedRequest.Name, []));

            while (toCheck.Count > 0)
            {
                var requestChain = toCheck.Pop();

                var chainSet = new HashSet<string>(requestChain.Chain, StringComparer.OrdinalIgnoreCase);
                if (chainSet.Contains(requestChain.RequestName))
                {
                    throw new ExitCodeException(ExitCode.RequestCyclicDependency, $"Detected request cyclic dependnecy on {requestChain.RequestName} starting from {queuedRequest.Name}.");
                }

                chainSet.Add(requestChain.RequestName);
                var request = simulationPayload.GetRequestConfiguration(requestChain.RequestName);
                if (request.RequestDependencies != null)
                {
                    foreach (var requestDep in request.RequestDependencies)
                    {
                        toCheck.Push(new CyclicChain(requestDep, chainSet));
                    }
                }
            }
        }
    }

    private class CyclicChain
    {
        public CyclicChain(string requestName, IEnumerable<string> chain)
        {
            RequestName = requestName;
            Chain = chain.ToArray();
        }

        public string RequestName { get; }
        public string[] Chain { get; }
    }
}
