using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
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

        services.AddKeyedTransient<IEventHandler<Request>, InitAgentEventHandler>(EventNames.InitAgent);
        services.AddKeyedTransient<IEventHandler<Request>, StartRequestEventHandler>(EventNames.StartRequest);
        services.AddKeyedTransient<IEventHandler<Request>, EndRequestEventHandler>(EventNames.FinishRequest);
        services.AddKeyedTransient<IEventHandler<Request>, CreateAgentEventHandler>(EventNames.CreateAgent);
        services.AddKeyedTransient<IEventHandler<RequestConfiguration>, QueueRequestEventHandler>(EventNames.QueueRequest);
    }
}
