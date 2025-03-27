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

namespace Dzaba.QueueSimulator.Lib.Tests;

[TestFixture]
public class SimulationEventsTests
{
    private static readonly DateTime CurrentTime = DateTime.Now;

    private IFixture fixture;
    private Mock<IRequestsRepository> requestRepo;
    private Mock<IAgentsRepository> agentsRepo;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
        requestRepo = fixture.FreezeMock<IRequestsRepository>();
        agentsRepo = fixture.FreezeMock<IAgentsRepository>();
    }

    private SimulationEvents CreateSut()
    {
        return fixture.Create<SimulationEvents>();
    }

    private void SetupIncludeAllAgentsAndRequests(bool include)
    {
        var settings = new SimulationSettings
        {
            RequestConfigurations = [],
            Agents = [],
            ReportSettings = new ReportSettings
            {
                IncludeAllAgents = include,
                IncludeAllRequests = include,
            }
        };
        fixture.FreezeMock<ISimulationContext>()
            .Setup(x => x.Payload)
            .Returns(new SimulationPayload(settings));
    }

    private void SetupRequestsQueue(params Request[] requests)
    {
        requestRepo.Setup(x => x.GetQueueLength())
            .Returns(requests.Length);
        requestRepo.Setup(x => x.GroupQueueByConfiguration())
            .Returns(requests.GroupByToArrayDict(b => b.RequestConfiguration, StringComparer.OrdinalIgnoreCase));
    }

    private void SetupRunningRequests(params Request[] requests)
    {
        requestRepo.Setup(x => x.GetRunningRequestCount())
            .Returns(requests.Length);
        requestRepo.Setup(x => x.GroupRunningRequestsByConfiguration())
            .Returns(requests.GroupByToArrayDict(b => b.RequestConfiguration, StringComparer.OrdinalIgnoreCase));
    }

    private void SetupAllRequests(params Request[] requests)
    {
        requestRepo.Setup(x => x.EnumerateRequests())
            .Returns(requests);
    }

    private void SetupAllAgents(params Agent[] agents)
    {
        agentsRepo.Setup(x => x.EnumerateAgents())
            .Returns(agents);
    }

    private void SetupActiveAgents(params Agent[] agents)
    {
        agentsRepo.Setup(x => x.GetActiveAgentsCount())
            .Returns(agents.Length);
        agentsRepo.Setup(x => x.GetActiveAgentsByConfigurationCount())
            .Returns(agents.GroupBy(b => b.AgentConfiguration, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count()));
    }

    private void SetupSomeData()
    {
        Request[] runningRequests = [
            new Request
            {
                AgentId = 1,
                Id = 1,
                CreatedTime = CurrentTime,
                RequestConfiguration = "BuildConfig1",
                State = RequestState.Running
            },
            new Request
            {
                AgentId = 2,
                Id = 2,
                CreatedTime = CurrentTime,
                RequestConfiguration = "BuildConfig2",
                State = RequestState.Running
            }
        ];

        Request[] queuedRequests = [
            new Request
            {
                AgentId = 3,
                Id = 3,
                CreatedTime = CurrentTime,
                RequestConfiguration = "BuildConfig1",
                State = RequestState.WaitingForAgent
            },
            new Request
            {
                AgentId = 4,
                Id = 4,
                CreatedTime = CurrentTime,
                RequestConfiguration = "BuildConfig2",
                State = RequestState.WaitingForAgent
            }
        ];

        Agent[] activeAgents = [
            new Agent
            {
                AgentConfiguration = "Agent1",
                State = AgentState.Running,
                Id = 1
            },
            new Agent
            {
                AgentConfiguration = "Agent2",
                State = AgentState.Running,
                Id = 2
            }
        ];

        SetupRequestsQueue(queuedRequests);
        SetupActiveAgents(activeAgents);
        SetupAllAgents(activeAgents);
        SetupRunningRequests(runningRequests);
        SetupAllRequests(queuedRequests.Concat(runningRequests).ToArray());
    }

    [Test]
    public void AddTimedEventData_WhenEventAdded_ThenAllDataAboutRequestAndAgentsIsIncluded()
    {
        SetupIncludeAllAgentsAndRequests(true);
        SetupSomeData();

        var sut = CreateSut();

        sut.AddTimedEventData(new EventData("TestEvent", CurrentTime), "TestMsg");

        sut.Should().HaveCount(1);
        var result = sut.First();

        result.AllAgents.Should().HaveCount(2);
        result.AllRequests.Should().HaveCount(4);
        result.RequestsQueue.Total.Should().Be(2);
        result.RequestsQueue.Grouped.Should().HaveCount(2);
        result.Message.Should().Be("TestMsg");
        result.Name.Should().Be("TestEvent");
        result.RunningAgents.Total.Should().Be(2);
        result.RunningAgents.Grouped.Should().HaveCount(2);
        result.RunningRequests.Total.Should().Be(2);
        result.RunningRequests.Grouped.Should().HaveCount(2);
        result.Timestamp.Should().Be(CurrentTime);
    }

    [Test]
    public void AddTimedEventData_WhenEventAddedAndSkipRequestsAndAgentrs_ThenADataAboutRequestAndAgentsIsSkipped()
    {
        SetupIncludeAllAgentsAndRequests(false);
        SetupSomeData();

        var sut = CreateSut();

        sut.AddTimedEventData(new EventData("TestEvent", CurrentTime), "TestMsg");

        sut.Should().HaveCount(1);
        var result = sut.First();

        result.AllAgents.Should().BeNull();
        result.AllRequests.Should().BeNull();
        result.RequestsQueue.Total.Should().Be(2);
        result.RequestsQueue.Grouped.Should().HaveCount(2);
        result.Message.Should().Be("TestMsg");
        result.Name.Should().Be("TestEvent");
        result.RunningAgents.Total.Should().Be(2);
        result.RunningAgents.Grouped.Should().HaveCount(2);
        result.RunningRequests.Total.Should().Be(2);
        result.RunningRequests.Grouped.Should().HaveCount(2);
        result.Timestamp.Should().Be(CurrentTime);
    }
}
