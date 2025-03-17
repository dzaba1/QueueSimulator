using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.QueueSimulator.Lib.Events;

internal sealed class StartRequestEventHandler : EventHandler<Request>
{
    private readonly ILogger<StartRequestEventHandler> logger;
    private readonly IAgentsRepository agentsRepo;
    private readonly ISimulationEventQueue eventQueue;
    private readonly ISimulationContext simulationContext;

    public StartRequestEventHandler(ISimulationEvents simulationEvents,
        ILogger<StartRequestEventHandler> logger,
        IAgentsRepository agentsRepo,
        ISimulationEventQueue eventQueue,
        ISimulationContext simulationContext)
        : base(simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(agentsRepo, nameof(agentsRepo));
        ArgumentNullException.ThrowIfNull(eventQueue, nameof(eventQueue));
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));

        this.logger = logger;
        this.agentsRepo = agentsRepo;
        this.eventQueue = eventQueue;
        this.simulationContext = simulationContext;
    }

    protected override string OnHandle(EventData eventData, Request payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var request = payload;

        logger.LogInformation("Starting the request {RequestId} [{Request}], Current time: {Time}",
            request.Id,
            request.RequestConfiguration,
            eventData.Time);

        var agent = agentsRepo.GetAgent(request.AgentId.Value);

        agent.State = AgentState.Running;
        request.StartTime = eventData.Time;
        request.State = RequestState.Running;

        var requestConfig = simulationContext.Payload.GetRequestConfiguration(request.RequestConfiguration);
        var requestEndTime = eventData.Time + requestConfig.Duration.Value;

        eventQueue.AddEndRequestQueueEvent(request, requestEndTime);

        return $"Started the request {request.Id} [{request.RequestConfiguration}] on agent {agent.Id} [{agent.AgentConfiguration}].";
    }
}
