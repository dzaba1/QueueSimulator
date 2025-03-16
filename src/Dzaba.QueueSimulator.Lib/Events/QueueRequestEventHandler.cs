using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Events;

internal sealed class QueueRequestEventPayload : EventDataPayload
{
    public QueueRequestEventPayload(EventData eventData, RequestConfiguration requestConfiguration)
        : base(eventData)
    {
        ArgumentNullException.ThrowIfNull(requestConfiguration, nameof(requestConfiguration));

        RequestConfiguration = requestConfiguration;
    }

    public RequestConfiguration RequestConfiguration { get; }
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

    protected override string OnHandle(QueueRequestEventPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var requestConfiguration = payload.RequestConfiguration;
        var eventData = payload.EventData;

        logger.LogInformation("Start queuening a new request {Request}, Current time: {Time}",
            requestConfiguration.Name, eventData.Time);

        var request = requestRepo.NewRequest(requestConfiguration, eventData.Time);
        if (requestConfiguration.RequestDependencies != null && requestConfiguration.RequestDependencies.Any())
        {
            request.State = RequestState.WaitingForDependencies;

            throw new NotImplementedException();
        }
        else
        {
            eventsQueue.AddCreateAgentQueueEvent(request, eventData.Time);
        }

        return $"Queued a new request {request.Id} [{request.RequestConfiguration}].";
    }
}
