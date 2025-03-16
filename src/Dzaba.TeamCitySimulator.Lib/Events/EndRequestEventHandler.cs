using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TeamCitySimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.TeamCitySimulator.Lib.Events;

internal sealed class EndRequestEventPayload : EventDataPayload
{
    public EndRequestEventPayload(EventData eventData, Request request)
        : base(eventData)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        Request = request;
    }

    public Request Request { get; }
}

internal sealed class EndRequestEventHandler : EventHandler<EndRequestEventPayload>
{
    private readonly ILogger<EndRequestEventHandler> logger;
    private readonly IAgentsRepository agentsRepo;
    private readonly IRequestsRepository requestRepo;
    private readonly ISimulationEventQueue eventQueue;

    public EndRequestEventHandler(ISimulationEvents simulationEvents,
        ILogger<EndRequestEventHandler> logger,
        IAgentsRepository agentsRepo,
        IRequestsRepository requestRepo,
        ISimulationEventQueue eventQueue)
        : base(simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(agentsRepo, nameof(agentsRepo));
        ArgumentNullException.ThrowIfNull(requestRepo, nameof(requestRepo));
        ArgumentNullException.ThrowIfNull(eventQueue, nameof(eventQueue));

        this.logger = logger;
        this.agentsRepo = agentsRepo;
        this.requestRepo = requestRepo;
        this.eventQueue = eventQueue;
    }

    protected override string OnHandle(EndRequestEventPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var request = payload.Request;
        var eventData = payload.EventData;

        logger.LogInformation("Start finishing request {RequestdId} [{Request}], Current time: {Time}",
            request.Id, request.RequestConfiguration, eventData.Time);

        var agent = agentsRepo.GetAgent(request.AgentId.Value);

        agent.State = AgentState.Finished;
        agent.EndTime = eventData.Time;
        request.EndTime = eventData.Time;
        request.State = RequestState.Finished;

        foreach (var scheduledRequest in requestRepo.GetWaitingForAgents())
        {
            eventQueue.AddCreateAgentQueueEvent(scheduledRequest, eventData.Time);
        }

        return $"Finished the build {request.Id} [{request.RequestConfiguration}] on agent {agent.Id} [{agent.AgentConfiguration}].";
    }
}
