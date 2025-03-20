using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.QueueSimulator.Lib.Events;

internal sealed class CreateAgentEventHandler : EventHandler<Request>
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

    protected override string OnHandle(EventData eventData, Request payload)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var request = payload;

        logger.LogInformation("Start creating an agent for request {RequestdId} [{Request}], Current time: {Time}",
            request.Id, request.RequestConfiguration, eventData.Time);

        var requestConfig = simulationContext.Payload.GetRequestConfiguration(request.RequestConfiguration);

        if (requestConfig.IsComposite)
        {
            throw new InvalidOperationException($"Request {request.Id} [{requestConfig.Name}] is composite. It can't be ran on agent.");
        }

        if (request.AgentId != null)
        {
            throw new InvalidOperationException($"Agent with ID {request.AgentId} was already assigned to request with ID {request.Id}.");
        }

        request.State = RequestState.WaitingForAgent;

        if (agentsRepo.TryCreateAgent(requestConfig.CompatibleAgents, eventData.Time, out var agent))
        {
            request.AgentId = agent.Id;
            request.State = RequestState.WaitingForAgentStart;

            eventsQueue.AddInitAgentQueueEvent(request, eventData.Time);

            return $"Created a new agent {agent.Id} [{agent.AgentConfiguration}] for request {request.Id} [{request.RequestConfiguration}].";
        }
        else
        {
            logger.LogInformation("There aren't any agents available for request {RequestdId} [{Request}].", request.Id, request.RequestConfiguration);
            return $"There aren't any agents available for request {request.Id} [{request.RequestConfiguration}].";
        }
    }
}
