using Dzaba.TeamCitySimulator.Lib.Model;
using Microsoft.Extensions.Logging;
using System;

namespace Dzaba.TeamCitySimulator.Lib.Events;

public interface ISimulationEventQueue
{
    void Run();
    void AddInitAgentQueueEvent(Build build, DateTime time);
    void AddStartBuildQueueEvent(Build build, DateTime time);
    void AddEndBuildQueueEvent(Build build, DateTime time);
    void AddCreateAgentQueueEvent(Build build, DateTime time);
    void AddQueueBuildQueueEvent(BuildConfiguration buildConfiguration, DateTime buildStartTime);
}

internal sealed class SimulationEventQueue : ISimulationEventQueue
{
    private readonly ILogger<SimulationEventQueue> logger;
    private readonly EventQueue eventsQueue = new();
    private readonly IEventHandlers eventHandlers;
    private readonly ISimulationContext simulationContext;

    public SimulationEventQueue(ILogger<SimulationEventQueue> logger,
        IEventHandlers eventHandlers,
        ISimulationContext simulationContext)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(eventHandlers, nameof(eventHandlers));
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));

        this.logger = logger;
        this.eventHandlers = eventHandlers;
        this.simulationContext = simulationContext;
    }

    public void Run()
    {
        while (eventsQueue.Count > 0)
        {
            eventsQueue.Dequeue().Invoke();
        }
    }

    public void AddInitAgentQueueEvent(Build build, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(build, nameof(build));

        logger.LogInformation("Adding agent init for [{BuildId}] {Build} for {Time} to the event queue.", build.Id, build.BuildConfiguration, time);

        eventsQueue.Enqueue(EventNames.InitAgent, time, e =>
        {
            var payload = new InitAgentEventPayload(e, build);
            var handler = eventHandlers.GetHandler<InitAgentEventPayload>();
            handler.Handle(payload);
        });
    }

    public void AddStartBuildQueueEvent(Build build, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(build, nameof(build));

        logger.LogInformation("Adding start build {Build} for {Time} to the event queue.", build.BuildConfiguration, time);

        eventsQueue.Enqueue(EventNames.StartBuild, time, e =>
        {
            var payload = new StartBuildEventPayload(e, build);
            var handler = eventHandlers.GetHandler<StartBuildEventPayload>();
            handler.Handle(payload);
        });
    }

    public void AddEndBuildQueueEvent(Build build, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(build, nameof(build));

        var payload = simulationContext.Payload;
        var buildConfig = payload.GetBuildConfiguration(build.BuildConfiguration);
        var buildEndTime = time + buildConfig.Duration.Value;

        logger.LogInformation("Adding finishing build {Build} for {Time} to the event queue.", build.BuildConfiguration, buildEndTime);

        eventsQueue.Enqueue(EventNames.FinishBuild, buildEndTime, e =>
        {
            var payload = new EndBuildEventPayload(e, build);
            var handler = eventHandlers.GetHandler<EndBuildEventPayload>();
            handler.Handle(payload);
        });
    }

    public void AddCreateAgentQueueEvent(Build build, DateTime time)
    {
        ArgumentNullException.ThrowIfNull(build, nameof(build));

        logger.LogInformation("Adding create agent for {Build} for {Time} to the event queue.", build.BuildConfiguration, time);

        eventsQueue.Enqueue(EventNames.CreateAgent, time, e =>
        {
            var payload = new CreateAgentEventPayload(e, build);
            var handler = eventHandlers.GetHandler<CreateAgentEventPayload>();
            handler.Handle(payload);
        });
    }

    public void AddQueueBuildQueueEvent(BuildConfiguration buildConfiguration, DateTime buildStartTime)
    {
        ArgumentNullException.ThrowIfNull(buildConfiguration, nameof(buildConfiguration));

        logger.LogInformation("Adding adding build {Build} for {Time} to the event queue.", buildConfiguration.Name, buildStartTime);

        eventsQueue.Enqueue(EventNames.QueueBuild, buildStartTime, e =>
        {
            var payload = new QueueBuildEventPayload(e, buildConfiguration);
            var handler = eventHandlers.GetHandler<QueueBuildEventPayload>();
            handler.Handle(payload);
        });
    }
}
