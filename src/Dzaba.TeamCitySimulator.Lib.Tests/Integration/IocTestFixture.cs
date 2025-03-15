using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using System;

namespace Dzaba.TeamCitySimulator.Lib.Tests.Integration;

public class IocTestFixture
{
    private ServiceProvider container;

    protected IServiceProvider Container => container;

    [SetUp]
    public void SetupContainer()
    {
        var services = new ServiceCollection();

        services.RegisterDzabaTeamCitySimulatorLib();

        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        services.AddLogging(l => l.AddSerilog(logger, true));

        container = services.BuildServiceProvider();
    }

    [TearDown]
    public void CleanContainer()
    {
        container?.Dispose();
    }
}
