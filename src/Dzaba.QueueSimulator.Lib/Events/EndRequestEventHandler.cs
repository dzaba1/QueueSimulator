using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Events;

internal sealed class EndRequestEventHandler : EventHandler<Request>
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

    protected override string OnHandle(EventData eventData, Request payload)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var request = payload;

        logger.LogInformation("Start finishing request {RequestdId} [{Request}], Current time: {Time}",
            request.Id, request.RequestConfiguration, eventData.Time);

        var agent = agentsRepo.GetAgent(request.AgentId.Value);

        agent.State = AgentState.Finished;
        agent.EndTime = eventData.Time;
        request.EndTime = eventData.Time;
        request.State = RequestState.Finished;

        EnqueueWaitingForAgents(eventData);
        EnqueueWaitingForDependencies(request, eventData);

        return $"Finished the request {request.Id} [{request.RequestConfiguration}] on agent {agent.Id} [{agent.AgentConfiguration}].";
    }

    private void EnqueueWaitingForDependencies(Request request, EventData eventData)
    {
        var pipeline = requestRepo.GetPipeline(request);
        var waitingRequests = pipeline.GetParents(request)
            .Where(r => r.State == RequestState.WaitingForDependencies);

        foreach (var waitingRequest in waitingRequests)
        {
            var children = pipeline.GetChildren(waitingRequest);
            if (children.All(r => r.State == RequestState.Finished))
            {
                logger.LogInformation("All dependencies of request {RequestdId} [{Request}] are finished.", waitingRequest.Id, waitingRequest.RequestConfiguration);
                waitingRequest.State = RequestState.Scheduled;
                eventQueue.AddCreateAgentQueueEvent(waitingRequest, eventData.Time);
            }
        }
    }

    private void EnqueueWaitingForAgents(EventData eventData)
    {
        foreach (var scheduledRequest in requestRepo.GetWaitingForAgents())
        {
            eventQueue.AddCreateAgentQueueEvent(scheduledRequest, eventData.Time);
        }
    }
}
