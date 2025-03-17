using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Events;

internal sealed class QueueRequestEventPayload
{
    public QueueRequestEventPayload(RequestConfiguration requestConfiguration,
        IPipeline pipeline,
        Request parent)
    {
        ArgumentNullException.ThrowIfNull(requestConfiguration, nameof(requestConfiguration));
        ArgumentNullException.ThrowIfNull(pipeline, nameof(pipeline));

        RequestConfiguration = requestConfiguration;
        Pipeline = pipeline;
        Parent = parent;
    }

    public RequestConfiguration RequestConfiguration { get; }
    public IPipeline Pipeline { get; }
    public Request Parent { get; }
}

internal sealed class QueueRequestEventHandler : EventHandler<QueueRequestEventPayload>
{
    private readonly ILogger<QueueRequestEventHandler> logger;
    private readonly IRequestsRepository requestRepo;
    private readonly ISimulationEventQueue eventsQueue;

    public QueueRequestEventHandler(ISimulationEvents simulationEvents,
        ILogger<QueueRequestEventHandler> logger,
        IRequestsRepository requestRepo,
        ISimulationEventQueue eventsQueue)
        : base(simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(requestRepo, nameof(requestRepo));
        ArgumentNullException.ThrowIfNull(eventsQueue, nameof(eventsQueue));

        this.logger = logger;
        this.requestRepo = requestRepo;
        this.eventsQueue = eventsQueue;
    }

    protected override string OnHandle(EventData eventData, QueueRequestEventPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var requestConfiguration = payload.RequestConfiguration;
        var pipeline = payload.Pipeline;

        logger.LogInformation("Start queuening a new request {Request}, Current time: {Time}",
            requestConfiguration.Name, eventData.Time);

        var request = GetOrCreateRequest(payload, eventData, out var created);

        if (created)
        {
            return EnqueueNew(request, payload, eventData);
        }

        return $"Skipping making a new request. There's an existing one {request.Id} [{requestConfiguration.Name}].";
    }

    private Request GetOrCreateRequest(QueueRequestEventPayload payload, EventData eventData, out bool created)
    {
        if (payload.Pipeline.TryGetRequest(payload.RequestConfiguration, out var request))
        {
            created = false;
        }
        else
        {
            request = requestRepo.NewRequest(payload.RequestConfiguration, payload.Pipeline, eventData.Time);
            created = true;
        }

        if (payload.Parent != null)
        {
            payload.Pipeline.SetReference(request, payload.Parent);
        }

        return request;
    }

    private void EnqueueStartRequest(Request request, DateTime time)
    {
        request.State = RequestState.Scheduled;
        eventsQueue.AddCreateAgentQueueEvent(request, time);
    }

    private string EnqueueNew(Request request, QueueRequestEventPayload payload, EventData eventData)
    {
        var children = payload.Pipeline.RequestConfigurationsGraph.GetChildren(payload.RequestConfiguration).ToArray();
        if (children.Length > 0)
        {
            request.State = RequestState.WaitingForDependencies;
            var allFinished = true;

            foreach (var child in children)
            {
                if (payload.Pipeline.TryGetRequest(child, out var childRequest))
                {
                    payload.Pipeline.SetReference(childRequest, request);

                    if (childRequest.State != RequestState.Finished)
                    {
                        allFinished = false;
                    }
                }
                else
                {
                    allFinished = false;

                    var childrenEventPayload = new QueueRequestEventPayload(child, payload.Pipeline, request);
                    eventsQueue.AddQueueRequestQueueEvent(childrenEventPayload, eventData.Time);
                }
            }

            if (allFinished)
            {
                EnqueueStartRequest(request, eventData.Time);
            }
        }
        else
        {
            EnqueueStartRequest(request, eventData.Time);
        }

        return $"Queued a new request {request.Id} [{request.RequestConfiguration}].";
    }
}
