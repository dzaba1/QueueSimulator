using AutoFixture;
using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Dzaba.TestUtils;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;

namespace Dzaba.QueueSimulator.Lib.Tests.Events;

[TestFixture]
public class StartRequestEventHandlerTests
{
    private static readonly DateTime CurrentTime = DateTime.Now;

    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private StartRequestEventHandler CreateSut()
    {
        return fixture.Create<StartRequestEventHandler>();
    }

    [Test]
    public void Handle_WhenHandled_ThenAgentAndRequestsAreRunningAndEndRequestEventIsScheduled()
    {
        var eventData = new EventData("Test", CurrentTime);
        var settings = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "Agent1"
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig",
                    Duration = TimeSpan.FromHours(1)
                }
            ],
        };
        fixture.SetupSimulationContext(settings);

        var agent = new Agent
        {
            Id = 1,
            AgentConfiguration = "Agent1"
        };

        var request = new Request
        {
            Id = 1,
            AgentId = agent.Id,
            RequestConfiguration = settings.RequestConfigurations[0].Name
        };

        fixture.FreezeMock<IAgentsRepository>()
            .Setup(x => x.GetAgent(agent.Id))
            .Returns(agent);

        var eventsPump = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, request);

        agent.State.Should().Be(AgentState.Running);
        request.State.Should().Be(RequestState.Running);
        request.StartTime.Should().Be(CurrentTime);

        eventsPump.Verify(x => x.AddEndRequestQueueEvent(request, CurrentTime + settings.RequestConfigurations[0].Duration.Value), Times.Once());
    }
}
