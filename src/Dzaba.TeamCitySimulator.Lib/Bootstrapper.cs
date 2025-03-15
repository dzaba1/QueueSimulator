using Dzaba.TeamCitySimulator.Lib.Events;
using Dzaba.TeamCitySimulator.Lib.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dzaba.TeamCitySimulator.Lib;

public static class Bootstrapper
{
    public static void RegisterDzabaTeamCitySimulatorLib(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddTransient<ISimulation, Simulation>();
        services.AddTransient<ISimulationValidation, SimulationValidation>();
        services.AddTransient<IEventHandlers, EventHandlers>();

        services.AddScoped<ISimulationContext, SimulationContext>();
        services.AddScoped<IAgentsRepository, AgentsRepository>();
        services.AddScoped<IBuildsRepository, BuildsRepository>();
        services.AddScoped<ISimulationEvents, SimulationEvents>();

        services.AddTransient<IEventHandler<InitAgentEventPayload>, InitAgentEventHandler>();
        services.AddTransient<IEventHandler<StartBuildEventPayload>, StartBuildEventHandler>();
        services.AddTransient<IEventHandler<EndBuildEventPayload>, EndBuildEventHandler>();
        services.AddTransient<IEventHandler<CreateAgentEventPayload>, CreateAgentEventHandler>();
        services.AddTransient<IEventHandler<QueueBuildEventPayload>, QueueBuildEventHandler>();
    }
}
