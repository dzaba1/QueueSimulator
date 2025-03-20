using AutoFixture;
using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Dzaba.TestUtils;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Tests.Events;

[TestFixture]
public class EndRequestEventHandlerTests
{
    private static readonly DateTime CurrentTime = DateTime.Now;

    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private EndRequestEventHandler CreateSut()
    {
        return fixture.Create<EndRequestEventHandler>();
    }

    [Test]
    public void Handle_WhenHandled_ThenAgentAndRequestIsFinished()
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

        var sut = CreateSut();

        sut.Handle(eventData, request);

        agent.State.Should().Be(AgentState.Finished);
        agent.EndTime.Should().Be(CurrentTime);
        request.State.Should().Be(RequestState.Finished);
        request.EndTime.Should().Be(CurrentTime);
    }

    [Test]
    public void Handle_WhenHandled_ThenWaitingForAgentRequestsAreRescheduled()
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
                    Duration = TimeSpan.FromHours(1),
                    CompatibleAgents = ["Agent1"]
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

        var agentsRepo = fixture.FreezeMock<IAgentsRepository>();
        agentsRepo.Setup(x => x.GetAgent(agent.Id))
            .Returns(agent);
        agentsRepo.Setup(x => x.CanAgentBeCreated(settings.RequestConfigurations[0].CompatibleAgents))
            .Returns(true);

        var watingRequests = fixture.CreateMany<Request>(3)
            .ForEachLazy(r => r.RequestConfiguration = "BuildConfig")
            .ToArray();
        fixture.FreezeMock<IRequestsRepository>()
            .Setup(x => x.GetWaitingForAgents())
            .Returns(watingRequests);

        var eventsPump = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, request);

        eventsPump.Verify(x => x.AddCreateAgentQueueEvent(It.Is<Request[]>(r => watingRequests.Length == r.Length), CurrentTime), Times.Exactly(1));
    }
}
