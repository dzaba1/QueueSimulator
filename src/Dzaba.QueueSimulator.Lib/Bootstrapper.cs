using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dzaba.QueueSimulator.Lib;

public static class Bootstrapper
{
    public static void RegisterDzabaQueueSimulatorLib(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddTransient<ISimulation, Simulation>();
        services.AddTransient<ISimulationValidation, SimulationValidation>();
        services.AddTransient<IEventHandlers, EventHandlers>();

        services.AddScoped<ISimulationContext, SimulationContext>();
        services.AddScoped<IAgentsRepository, AgentsRepository>();
        services.AddScoped<IRequestsRepository, RequestsRepository>();
        services.AddScoped<ISimulationEvents, SimulationEvents>();
        services.AddScoped<ISimulationEventQueue, SimulationEventQueue>();

        services.AddTransient<IEventHandler<InitAgentEventPayload>, InitAgentEventHandler>();
        services.AddTransient<IEventHandler<StartRequestEventPayload>, StartRequestEventHandler>();
        services.AddTransient<IEventHandler<EndRequestEventPayload>, EndRequestEventHandler>();
        services.AddTransient<IEventHandler<CreateAgentEventPayload>, CreateAgentEventHandler>();
        services.AddTransient<IEventHandler<QueueRequestEventPayload>, QueueRequestEventHandler>();
    }
}
