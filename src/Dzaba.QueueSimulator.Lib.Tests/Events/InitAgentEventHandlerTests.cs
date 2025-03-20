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
public class InitAgentEventHandlerTests
{
    private static readonly DateTime CurrentTime = DateTime.Now;

    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private InitAgentEventHandler CreateSut()
    {
        return fixture.Create<InitAgentEventHandler>();
    }

    [Test]
    public void OnHandle_WhenNoInitTime_ThenAgentIsCreatedImmedietly()
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
            RequestConfigurations = [],
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
        };

        fixture.FreezeMock<IAgentsRepository>()
            .Setup(x => x.GetAgent(agent.Id))
            .Returns(agent);

        var eventsPump = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, request);

        agent.State.Should().Be(AgentState.Initiating);
        eventsPump.Verify(x => x.AddAgentInitedQueueEvent(request, CurrentTime), Times.Once());
    }

    [Test]
    public void OnHandle_WhenInitTime_ThenAgentIsCreatedAfterSomeTime()
    {
        var eventData = new EventData("Test", CurrentTime);
        var settings = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "Agent1",
                    InitTime = TimeSpan.FromMinutes(1)
                }
            ],
            RequestConfigurations = [],
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
        };

        fixture.FreezeMock<IAgentsRepository>()
            .Setup(x => x.GetAgent(agent.Id))
            .Returns(agent);

        var eventsPump = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, request);

        agent.State.Should().Be(AgentState.Initiating);
        eventsPump.Verify(x => x.AddAgentInitedQueueEvent(request, CurrentTime + settings.Agents[0].InitTime.Value), Times.Once());
    }
}
