using Dzaba.QueueSimulator.Lib.Model;
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

        var errors = new List<KeyValuePair<ExitCode, string>>();

        foreach (var request in simulationPayload.RequestConfigurationsCached.Values)
        {
            ValidateCompatibleAgents(request, simulationPayload, errors);
            ValidateRequestDependencies(request, simulationPayload, errors); 
          
            if (request.IsComposite && request.Duration != null)
            {
                errors.Add(new KeyValuePair<ExitCode, string>(ExitCode.CompositeWithDuration, $"The composite request definition {request.Name} has some duration."));
            }
        }

        ValidateCyclicDependency(simulationPayload, errors);

        if (simulationPayload.SimulationSettings.RequestConfigurationsToObserve != null)
        {
            ValidateRequestsExist(simulationPayload, simulationPayload.SimulationSettings.RequestConfigurationsToObserve, errors);
        }

        if (errors.Any())
        {
            throw new ExitCodeException(errors);
        }
    }

    private void ValidateCyclicDependency(SimulationPayload simulationPayload, IList<KeyValuePair<ExitCode, string>> errors)
    {
        foreach (var queuedRequest in simulationPayload.SimulationSettings.InitialRequests)
        {
            if (!simulationPayload.RequestConfigurationsCached.ContainsKey(queuedRequest.Name))
            {
                errors.Add(new KeyValuePair<ExitCode, string>(ExitCode.RequestNotFound, $"Couldn't find request configuration {queuedRequest.Name}."));
                continue;
            }

            Stack<CyclicChain> toCheck = new Stack<CyclicChain>();
            toCheck.Push(new CyclicChain(queuedRequest.Name, []));

            while (toCheck.Count > 0)
            {
                var requestChain = toCheck.Pop();

                var chainSet = new HashSet<string>(requestChain.Chain, StringComparer.OrdinalIgnoreCase);
                if (chainSet.Contains(requestChain.RequestName))
                {
                    errors.Add(new KeyValuePair<ExitCode, string>(ExitCode.RequestCyclicDependency, $"Detected request cyclic dependnecy on {requestChain.RequestName} starting from {queuedRequest.Name}."));
                    break;
                }

                chainSet.Add(requestChain.RequestName);
                if (simulationPayload.RequestConfigurationsCached.TryGetValue(requestChain.RequestName, out var request))
                {
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
    }

    private void ValidateRequestsExist(SimulationPayload simulationPayload,
        IEnumerable<string> requests,
        IList<KeyValuePair<ExitCode, string>> errors)
    {
        foreach (var requestName in requests)
        {
            if (!simulationPayload.RequestConfigurationsCached.ContainsKey(requestName))
            {
                errors.Add(new KeyValuePair<ExitCode, string>(ExitCode.RequestNotFound, $"Couldn't find request configuration {requestName}."));
            }
        }
    }

    private void ValidateRequestDependencies(RequestConfiguration request,
        SimulationPayload simulationPayload,
        IList<KeyValuePair<ExitCode, string>> errors)
    {
        if (request.IsComposite && (request.RequestDependencies == null || request.RequestDependencies.Length == 0))
        {
            errors.Add(new KeyValuePair<ExitCode, string>(ExitCode.CompositeWithoutDependencies, $"The composite request definition {request.Name} doesn't have any dependencies."));
        }

        if (request.RequestDependencies != null)
        {
            ValidateRequestsExist(simulationPayload, request.RequestDependencies, errors);
        }
    }

    private void ValidateCompatibleAgents(RequestConfiguration request,
        SimulationPayload simulationPayload,
        IList<KeyValuePair<ExitCode, string>> errors)
    {
        if (request.CompatibleAgents != null && request.CompatibleAgents.Length > 0)
        {
            if (request.IsComposite)
            {
                errors.Add(new KeyValuePair<ExitCode, string>(ExitCode.CompositeWithAgents, $"The composite request definition {request.Name} has some agents defined."));
                return;
            }

            foreach (var agentName in request.CompatibleAgents)
            {
                if (!simulationPayload.AgentConfigurationsCached.ContainsKey(agentName))
                {
                    errors.Add(new KeyValuePair<ExitCode, string>(ExitCode.AgentNotFound, $"Couldn't find agent {agentName} for request configuration {request.Name}."));
                }
            }
        }
        else if (!request.IsComposite)
        {
            errors.Add(new KeyValuePair<ExitCode, string>(ExitCode.RequestWithoutAgents, $"The request definition {request.Name} doesn't have agents defined."));
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
