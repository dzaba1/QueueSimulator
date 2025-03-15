using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TeamCitySimulator.Lib.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dzaba.TeamCitySimulator.Lib.Events;

internal sealed class QueueBuildEventPayload : EventDataPayload
{
    public QueueBuildEventPayload(EventData eventData, BuildConfiguration buildConfiguration)
        : base(eventData)
    {
        ArgumentNullException.ThrowIfNull(buildConfiguration, nameof(buildConfiguration));

        BuildConfiguration = buildConfiguration;
    }

    public BuildConfiguration BuildConfiguration { get; }
}

internal sealed class QueueBuildEventHandler : EventHandler<QueueBuildEventPayload>
{
    private readonly ILogger<QueueBuildEventHandler> logger;
    private readonly IBuildsRepository buildRepo;
    private readonly ISimulationEventQueue eventsQueue;

    public QueueBuildEventHandler(ISimulationEvents simulationEvents,
        ILogger<QueueBuildEventHandler> logger,
        IBuildsRepository buildRepo,
        ISimulationEventQueue eventsQueue)
        : base(simulationEvents)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(buildRepo, nameof(buildRepo));
        ArgumentNullException.ThrowIfNull(eventsQueue, nameof(eventsQueue));

        this.logger = logger;
        this.buildRepo = buildRepo;
        this.eventsQueue = eventsQueue;
    }

    protected override string OnHandle(QueueBuildEventPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload, nameof(payload));

        var buildConfiguration = payload.BuildConfiguration;
        var eventData = payload.EventData;

        logger.LogInformation("Start queuening a new build {Build}, Current time: {Time}", buildConfiguration.Name, eventData.Time);

        var build = buildRepo.NewBuild(buildConfiguration, eventData.Time);
        if (buildConfiguration.BuildDependencies != null && buildConfiguration.BuildDependencies.Any())
        {
            build.State = BuildState.WaitingForDependencies;

            throw new NotImplementedException();
        }
        else
        {
            eventsQueue.AddCreateAgentQueueEvent(build, eventData.Time);
        }

        return $"Queued a new build [{build.Id}] {build.BuildConfiguration}.";
    }
}
