using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Dzaba.QueueSimulator.Lib.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Events;

internal sealed class EndRequestEventHandler : EventHandler<Request>
{
    private readonly ILogger<EndRequestEventHandler> logger;
    private readonly IAgentsRepository agentsRepo;
    private readonly IRequestsRepository requestRepo;
    private readonly ISimulationEventQueue eventQueue;
    private readonly ISimulationContext simulationContext;

    public EndRequestEventHandler(ISimulationEvents simulationEvents,
        ILogger<EndRequestEventHandler> logger,
        IAgentsRepository agentsRepo,
        IRequestsRepository requestRepo,
        ISimulationEventQueue eventQueue,
        ISimulationContext simulationContext)
        : base(simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(agentsRepo, nameof(agentsRepo));
        ArgumentNullException.ThrowIfNull(requestRepo, nameof(requestRepo));
        ArgumentNullException.ThrowIfNull(eventQueue, nameof(eventQueue));
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));

        this.logger = logger;
        this.agentsRepo = agentsRepo;
        this.requestRepo = requestRepo;
        this.eventQueue = eventQueue;
        this.simulationContext = simulationContext;
    }

    protected override string OnHandle(EventData eventData, Request payload)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var request = payload;

        logger.LogInformation("Start finishing request {RequestdId} [{Request}], Current time: {Time}",
            request.Id, request.RequestConfiguration, eventData.Time);

        var requestConfiguration = simulationContext.Payload.RequestConfigurations.GetEntity(request.RequestConfiguration);

        request.EndTime = eventData.Time;
        request.State = RequestState.Finished;

        var toEnqueue = new HashSet<Request>(new OnePropertyComparer<Request, long>(r => r.Id));

        if (!requestConfiguration.IsComposite)
        {
            var agent = agentsRepo.GetAgent(request.AgentId.Value);
            agent.State = AgentState.Finished;
            agent.EndTime = eventData.Time;

            EnqueueWaitingForAgents(eventData, toEnqueue);
        }

        EnqueueWaitingForDependencies(request, eventData, toEnqueue);

        ReEnqueueAll(toEnqueue, eventData);

        return $"Finished the request {request.Id} [{request.RequestConfiguration}].";
    }

    private void EnqueueWaitingForDependencies(Request request, EventData eventData, HashSet<Request> toEnqueue)
    {
        var pipeline = requestRepo.GetPipeline(request);
        var waitingRequests = pipeline.GetParents(request, false)
            .Select(r => new RequestWithConfiguration
            {
                Request = r,
                RequestConfiguration = simulationContext.Payload.RequestConfigurations.GetEntity(r.RequestConfiguration)
            })
            .Where(r => {
                return r.Request.State == RequestState.WaitingForDependencies ||
                    (r.RequestConfiguration.IsComposite && r.Request.State == RequestState.Running);
            });

        foreach (var waitingRequest in waitingRequests)
        {
            var children = pipeline.GetChildren(waitingRequest.Request);
            if (children.All(r => r.State == RequestState.Finished))
            {
                logger.LogInformation("All dependencies of request {RequestdId} [{Request}] are finished.",
                    waitingRequest.Request.Id, waitingRequest.RequestConfiguration.Name);

                if (waitingRequest.RequestConfiguration.IsComposite)
                {
                    eventQueue.AddEndRequestQueueEvent(waitingRequest.Request, eventData.Time);
                }
                else
                {
                    waitingRequest.Request.State = RequestState.WaitingForAgent;
                    toEnqueue.Add(waitingRequest.Request);
                }
            }
        }
    }

    private void EnqueueWaitingForAgents(EventData eventData, HashSet<Request> toEnqueue)
    {
        foreach (var scheduledRequest in requestRepo.GetWaitingForAgents())
        {
            toEnqueue.Add(scheduledRequest);
        }
    }

    private void ReEnqueueAll(IEnumerable<Request> requests, EventData eventData)
    {
        if (agentsRepo.MaxAgentsReached())
        {
            logger.LogInformation("Max agents reached. Skipping re-enqueue all.");
            return;
        }

        eventQueue.AddCreateAgentQueueEvent(requests.ToArray(), eventData.Time);
    }
}
