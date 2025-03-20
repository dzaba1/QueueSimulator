using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Events;

internal sealed class CreateAgentEventHandler : EventHandler<Request[]>
{
    private readonly ILogger<CreateAgentEventHandler> logger;
    private readonly ISimulationContext simulationContext;
    private readonly IAgentsRepository agentsRepo;
    private readonly ISimulationEventQueue eventsQueue;

    public CreateAgentEventHandler(ISimulationEvents simulationEvents,
        ILogger<CreateAgentEventHandler> logger,
        ISimulationContext simulationContext,
        IAgentsRepository agentsRepo,
        ISimulationEventQueue eventsQueue)
        : base(simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));
        ArgumentNullException.ThrowIfNull(agentsRepo, nameof(agentsRepo));
        ArgumentNullException.ThrowIfNull(eventsQueue, nameof(eventsQueue));

        this.logger = logger;
        this.simulationContext = simulationContext;
        this.agentsRepo = agentsRepo;
        this.eventsQueue = eventsQueue;
    }

    protected override string OnHandle(EventData eventData, Request[] payload)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        if (agentsRepo.MaxAgentsReached())
        {
            return "Max running agents reached.";
        }

        var filtered = Filter(payload, eventData)
            .OrderBy(r => r.Request.Id);

        foreach (var request in filtered)
        {
            if (agentsRepo.MaxAgentsReached())
            {
                break;
            }

            if (agentsRepo.TryCreateAgent(request.RequestConfiguration.CompatibleAgents, eventData.Time, out var agent))
            {
                request.Request.AgentId = agent.Id;
                request.Request.State = RequestState.WaitingForAgentStart;

                eventsQueue.AddInitAgentQueueEvent(request.Request, eventData.Time);

                logger.LogInformation("Created a new agent {AgentId} [{Agent}] for request {RequestId} [{Request}].",
                    agent.Id, agent.AgentConfiguration,
                    request.Request.Id, request.RequestConfiguration.Name);
            }
            else
            {
                logger.LogInformation("There aren't any agents available for request {RequestdId} [{Request}].",
                    request.Request.Id, request.Request.RequestConfiguration);
            }
        }

        return "Created agents event.";
    }

    private IEnumerable<RequestWithConfiguration> Filter(Request[] payload, EventData eventData)
    {
        foreach (var request in payload)
        {
            var requestConfig = simulationContext.Payload.GetRequestConfiguration(request.RequestConfiguration);

            if (requestConfig.IsComposite)
            {
                throw new InvalidOperationException($"Request {request.Id} [{requestConfig.Name}] is composite. It can't be ran on agent.");
            }

            if (request.State < RequestState.WaitingForDependencies)
            {
                throw new InvalidOperationException($"Request {request.Id} [{request.RequestConfiguration}] is in {request.State} state.");
            }

            if (IsRunningOrFinished(request))
            {
                logger.LogInformation("Request {RequestdId} [{Request}] is already in {RequestState} state.",
                    request.Id, request.RequestConfiguration, request.State);
                continue;
            }

            if (request.AgentId != null)
            {
                throw new InvalidOperationException($"Agent with ID {request.AgentId} was already assigned to request with ID {request.Id}.");
            }

            logger.LogInformation("Start creating an agent for request {RequestdId} [{Request}], Current time: {Time}",
                request.Id, request.RequestConfiguration, eventData.Time);

            yield return new RequestWithConfiguration
            {
                Request = request,
                RequestConfiguration = requestConfig
            };
        }
    }

    private bool IsRunningOrFinished(Request request)
    {
        return request.State >= RequestState.WaitingForAgentStart;
    }
}
