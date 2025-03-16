using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

namespace Dzaba.QueueSimulator.Lib.Tests.Integration;

[TestFixture]
public class EventHandlersTests : IocTestFixture
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IEventHandler<TestEventPayload>, TestEventHandler>();
    }

    private IEventHandler<TestEventPayload> GetHandler(IServiceScope scope, SimulationSettings settings)
    {
        var context = scope.ServiceProvider.GetRequiredService<ISimulationContext>();
        context.Payload.Should().BeNull();

        context.SetSettings(settings);

        var sut = scope.ServiceProvider.GetRequiredService<IEventHandlers>();

        return sut.GetHandler<TestEventPayload>();
    }

    private void TestScope(IEventHandler<TestEventPayload> handler, SimulationSettings settings)
    {
        var payload = new TestEventPayload(new EventData("Test", DateTime.Now), settings);

        handler.Handle(payload);

        payload.ActualPayload.SimulationSettings.Should().BeSameAs(settings);
        payload.ActualPayload.SimulationSettings.Should().BeSameAs(payload.ExpectedSettings);
    }

    [Test]
    public void GetHandler_WhenRequested_ThenSettingsAreScoped()
    {
        var settings1 = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1"
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    Duration = TimeSpan.FromMinutes(1)
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "BuildConfig1",
                    NumberToQueue = 1
                }
            ]
        };

        var settings2 = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent2"
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig2",
                    CompatibleAgents = ["TestAgent2"],
                    Duration = TimeSpan.FromMinutes(1)
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "BuildConfig2",
                    NumberToQueue = 1
                }
            ]
        };

        using var scope1 = Container.CreateScope();
        using var scope2 = Container.CreateScope();

        var handler1 = GetHandler(scope1, settings1);
        var handler2 = GetHandler(scope2, settings2);

        TestScope(handler1, settings1);
        TestScope(handler2, settings2);
    }

    private class TestEventPayload : EventDataPayload
    {
        public TestEventPayload(EventData eventData,
            SimulationSettings expectedSettings) : base(eventData)
        {
            ArgumentNullException.ThrowIfNull(expectedSettings, nameof(expectedSettings));

            ExpectedSettings = expectedSettings;
        }

        public SimulationSettings ExpectedSettings { get; }

        public SimulationPayload ActualPayload { get; set; }
    }

    private class TestEventHandler : Lib.Events.EventHandler<TestEventPayload>
    {
        private readonly ISimulationContext simulationContext;

        public TestEventHandler(ISimulationEvents simulationEvents,
            ISimulationContext simulationContext) : base(simulationEvents)
        {
            ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));

            this.simulationContext = simulationContext;
        }

        protected override string OnHandle(TestEventPayload payload)
        {
            payload.ActualPayload = simulationContext.Payload;

            return "Test";
        }
    }
}
