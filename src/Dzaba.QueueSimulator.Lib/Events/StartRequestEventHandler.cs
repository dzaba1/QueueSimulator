using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Dzaba.QueueSimulator.Lib.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Events;

internal sealed class StartRequestEventHandler : EventHandler<Request>
{
    private readonly ILogger<StartRequestEventHandler> logger;
    private readonly IAgentsRepository agentsRepo;
    private readonly ISimulationEventQueue eventQueue;
    private readonly ISimulationContext simulationContext;
    private readonly IRequestsRepository requestsRepo;

    public StartRequestEventHandler(ISimulationEvents simulationEvents,
        ILogger<StartRequestEventHandler> logger,
        IAgentsRepository agentsRepo,
        ISimulationEventQueue eventQueue,
        ISimulationContext simulationContext,
        IRequestsRepository requestsRepo)
        : base(simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(agentsRepo, nameof(agentsRepo));
        ArgumentNullException.ThrowIfNull(eventQueue, nameof(eventQueue));
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));
        ArgumentNullException.ThrowIfNull(requestsRepo, nameof(requestsRepo));

        this.logger = logger;
        this.agentsRepo = agentsRepo;
        this.eventQueue = eventQueue;
        this.simulationContext = simulationContext;
        this.requestsRepo = requestsRepo;
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

        SetCompositeStart(request, eventData);

        var requestConfig = simulationContext.Payload.GetRequestConfiguration(request.RequestConfiguration);
        var requestEndTime = eventData.Time + requestConfig.Duration.Value;

        eventQueue.AddEndRequestQueueEvent(request, requestEndTime);

        return $"Started the request {request.Id} [{request.RequestConfiguration}] on agent {agent.Id} [{agent.AgentConfiguration}].";
    }

    private void SetCompositeStart(Request request, EventData eventData)
    {
        var pipeline = requestsRepo.GetPipeline(request);

        var toStart = pipeline.GetParents(request, true)
            .Distinct(new OnePropertyComparer<Request, long>(r => r.Id))
            .Select(r => new RequestWithConfiguration
            {
                Request = r,
                RequestConfiguration = simulationContext.Payload.GetRequestConfiguration(r.RequestConfiguration)
            })
            .Where(r => r.RequestConfiguration.IsComposite)
            .Where(r => r.Request.State == RequestState.WaitingForDependencies)
            .Select(r => r.Request);

        foreach (var compositeRequest in toStart)
        {
            logger.LogDebug("Starting composite request {RequestId} [{Request}], Current time: {Time}",
                compositeRequest.Id,
                compositeRequest.RequestConfiguration,
                eventData.Time);

            compositeRequest.StartTime = eventData.Time;
            compositeRequest.State = RequestState.Running;
        }
    }
}
