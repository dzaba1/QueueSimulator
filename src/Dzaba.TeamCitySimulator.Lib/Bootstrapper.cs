using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dzaba.TeamCitySimulator.Lib;

public static class Bootstrapper
{
    public static void RegisterDzabaTeamCitySimulatorLib(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddTransient<ISimulation, Simulation>();
    }
}
