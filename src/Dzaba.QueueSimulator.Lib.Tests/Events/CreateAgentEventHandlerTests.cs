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
public class CreateAgentEventHandlerTests
{
    private static readonly DateTime CurrentTime = DateTime.Now;

    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private CreateAgentEventHandler CreateSut()
    {
        return fixture.Create<CreateAgentEventHandler>();
    }

    private void SetupContext(SimulationSettings settings)
    {
        fixture.FreezeMock<ISimulationContext>()
            .Setup(x => x.Payload).Returns(new SimulationPayload(settings));
    }

    [Test]
    public void Handle_WhenAgentCreated_ThenNextInitIt()
    {
        var eventData = new EventData("TestEvent", CurrentTime);
        var request = new Request
        {
            RequestConfiguration = "BuildConfig1"
        };

        var settings = new SimulationSettings
        {
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = request.RequestConfiguration,
                    CompatibleAgents = ["Agent1"]
                }
            ],
            Agents = []
        };

        SetupContext(settings);

        var agent = new Agent
        {
            Id = 1
        };

        fixture.FreezeMock<IAgentsRepository>()
            .Setup(x => x.TryCreateAgent(settings.RequestConfigurations[0].CompatibleAgents, CurrentTime, out agent))
            .Returns(true);

        var eventsPump = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, request);

        request.State.Should().Be(RequestState.WaitingForAgent);
        request.AgentId.Should().Be(agent.Id);
        eventsPump.Verify(x => x.AddInitAgentQueueEvent(request, CurrentTime), Times.Once());
    }

    [Test]
    public void Handle_WhenAgentNotCreated_ThenNothing()
    {
        var eventData = new EventData("TestEvent", CurrentTime);
        var request = new Request
        {
            RequestConfiguration = "BuildConfig1"
        };

        var settings = new SimulationSettings
        {
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = request.RequestConfiguration,
                    CompatibleAgents = ["Agent1"]
                }
            ],
            Agents = []
        };

        SetupContext(settings);

        Agent agent = null;

        fixture.FreezeMock<IAgentsRepository>()
            .Setup(x => x.TryCreateAgent(settings.RequestConfigurations[0].CompatibleAgents, CurrentTime, out agent))
            .Returns(false);

        var sut = CreateSut();

        sut.Handle(eventData, request);

        request.State.Should().Be(RequestState.WaitingForAgent);
        request.AgentId.Should().BeNull();
    }
}
