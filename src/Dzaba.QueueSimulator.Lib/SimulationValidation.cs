﻿using Dzaba.QueueSimulator.Lib.Model;
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
            ValidateCompatibleAgents(request, simulationPayload);
            ValidateRequestDependencies(request, simulationPayload); 
          
            if (request.IsComposite && request.Duration != null)
            {
                throw new ExitCodeException(ExitCode.CompositeWithDuration, $"The composite request definition {request.Name} has some duration.");
            }
        }

        ValidateCyclicDependency(simulationPayload);
    }

    private void ValidateCyclicDependency(SimulationPayload simulationPayload)
    {
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

    private void ValidateRequestDependencies(RequestConfiguration request, SimulationPayload simulationPayload)
    {
        if (request.IsComposite && (request.RequestDependencies == null || request.RequestDependencies.Length == 0))
        {
            throw new ExitCodeException(ExitCode.CompositeWithoutDependencies, $"The composite request definition {request.Name} doesn't have any dependencies.");
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

    private void ValidateCompatibleAgents(RequestConfiguration request, SimulationPayload simulationPayload)
    {
        if (request.CompatibleAgents != null && request.CompatibleAgents.Length > 0)
        {
            if (request.IsComposite)
            {
                throw new ExitCodeException(ExitCode.CompositeWithAgents, $"The composite request definition {request.Name} has some agents defined.");
            }

            foreach (var agentName in request.CompatibleAgents)
            {
                if (!simulationPayload.AgentConfigurationsCached.ContainsKey(agentName))
                {
                    throw new ExitCodeException(ExitCode.AgentNotFound, $"Couldn't find agent {agentName} for request configuration {request.Name}.");
                }
            }
        }
        else if (!request.IsComposite)
        {
            throw new ExitCodeException(ExitCode.RequestWithoutAgents, $"The request definition {request.Name} doesn't have agents defined.");
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
