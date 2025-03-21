using AutoFixture;
using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.TestUtils;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;

namespace Dzaba.QueueSimulator.Lib.Tests;

[TestFixture]
public class SimulationTests
{
    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private Simulation CreateSut()
    {
        return fixture.Create<Simulation>();
    }

    private SimulationSettings GetSomeSettings()
    {
        return new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1",
                    InitTime = TimeSpan.FromMinutes(15)
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
                    NumberToQueue = 8
                }
            ]
        };
    }

    [Test]
    public void Run_WhenCalled_ThenContextIsSet()
    {
        var settings = GetSomeSettings();
        var context = fixture.SetupSimulationContext(settings);

        var sut = CreateSut();

        sut.Run(settings);

        context.Verify(x => x.SetSettings(settings), Times.Once());
    }

    [Test]
    public void Run_WhenCalled_ThenEventPumpIsCalled()
    {
        var settings = GetSomeSettings();
        fixture.SetupSimulationContext(settings);

        var eventPump = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Run(settings);

        eventPump.Verify(x => x.Run(), Times.Once());
    }

    private bool VerifyPayload(QueueRequestEventPayload actual, RequestConfiguration expected)
    {
        return actual.RequestConfiguration == expected && actual.Parent == null;
    }

    [Test]
    public void Run_WhenInitRequests_ThenThoseAreDividedWithConstantInterval()
    {
        var settings = GetSomeSettings();
        fixture.SetupSimulationContext(settings);

        var eventPump = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Run(settings);

        eventPump.Verify(x => x.AddQueueRequestQueueEvent(It.Is<QueueRequestEventPayload>(p => VerifyPayload(p, settings.RequestConfigurations[0])),
            It.IsAny<DateTimeOffset>()), Times.Exactly(8));

        for (int i = 0; i < 8; i++)
        {
            var expected = Simulation.StartTime.AddHours(1 * i);
            eventPump.Verify(x => x.AddQueueRequestQueueEvent(It.Is<QueueRequestEventPayload>(p => VerifyPayload(p, settings.RequestConfigurations[0])),
                expected), Times.Once());
        }
    }

    [Test]
    public void Run_WhenCalled_ThenEventsAreReturned()
    {
        var settings = GetSomeSettings();
        fixture.SetupSimulationContext(settings);

        var events = fixture.FreezeMock<ISimulationEvents>();

        var sut = CreateSut();

        var result = sut.Run(settings);

        result.Events.Should().BeEquivalentTo(events.Object);
    }
}
