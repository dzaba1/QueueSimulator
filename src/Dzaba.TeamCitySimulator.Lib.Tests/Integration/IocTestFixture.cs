using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using System;

namespace Dzaba.QueueSimulator.Lib.Tests.Integration;

public class IocTestFixture
{
    private ServiceProvider container;

    protected IServiceProvider Container => container;

    [SetUp]
    public void SetupContainer()
    {
        var services = new ServiceCollection();

        services.RegisterDzabaQueueSimulatorLib();

        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        services.AddLogging(l => l.AddSerilog(logger, true));

        RegisterServices(services);

        container = services.BuildServiceProvider();
    }

    protected virtual void RegisterServices(IServiceCollection services)
    {

    }

    [TearDown]
    public void CleanContainer()
    {
        container?.Dispose();
    }
}
