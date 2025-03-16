using AutoFixture;
using Dzaba.TeamCitySimulator.Lib.Events;
using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TeamCitySimulator.Lib.Repositories;
using Dzaba.TestUtils;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib.Tests;

[TestFixture]
public class SimulationEventsTests
{
    private static readonly DateTime CurrentTime = DateTime.Now;

    private IFixture fixture;
    private Mock<IBuildsRepository> buildsRepo;
    private Mock<IAgentsRepository> agentsRepo;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
        buildsRepo = fixture.FreezeMock<IBuildsRepository>();
        agentsRepo = fixture.FreezeMock<IAgentsRepository>();
    }

    private SimulationEvents CreateSut()
    {
        return fixture.Create<SimulationEvents>();
    }

    private void SetupIncludeAllAgentsAndBuilds(bool include)
    {
        var settings = new SimulationSettings
        {
            IncludeAllAgents = include,
            IncludeAllBuilds = include,
            BuildConfigurations = [],
            Agents = [],
        };
        fixture.FreezeMock<ISimulationContext>()
            .Setup(x => x.Payload).Returns(new SimulationPayload(settings));
    }

    private void SetupBuildsQueue(params Build[] builds)
    {
        buildsRepo.Setup(x => x.GetQueueLength())
            .Returns(builds.Length);
        buildsRepo.Setup(x => x.GroupQueueByBuildConfiguration())
            .Returns(builds.GroupByToArrayDict(b => b.BuildConfiguration, StringComparer.OrdinalIgnoreCase));
    }

    private void SetupRunningBuilds(params Build[] builds)
    {
        buildsRepo.Setup(x => x.GetRunningBuildsCount())
            .Returns(builds.Length);
        buildsRepo.Setup(x => x.GroupRunningBuildsByBuildConfiguration())
            .Returns(builds.GroupByToArrayDict(b => b.BuildConfiguration, StringComparer.OrdinalIgnoreCase));
    }

    private void SetupAllBuilds(params Build[] builds)
    {
        buildsRepo.Setup(x => x.EnumerateBuilds())
            .Returns(builds);
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

    [Test]
    public void AddTimedEventData_WhenEventAdded_ThenAllDataAboutBuildsAndAgentsIsIncluded()
    {
        SetupIncludeAllAgentsAndBuilds(true);

        var sut = CreateSut();

        sut.AddTimedEventData(new EventData("TestEvent", CurrentTime), "TestMsg");

        sut.Should().HaveCount(1);
        var result = sut.First();

        result.AllAgents.Should().HaveCount(4);
        result.AllBuilds.Should().HaveCount(4);
        result.BuildsQueue.Total.Should().Be(2);
        result.BuildsQueue.Grouped.Should().HaveCount(2);
        result.Message.Should().Be("TestMsg");
        result.Name.Should().Be("TestEvent");
        result.RunningAgents.Total.Should().Be(2);
        result.RunningAgents.Grouped.Should().HaveCount(2);
        result.RunningBuilds.Total.Should().Be(2);
        result.RunningBuilds.Grouped.Should().HaveCount(2);
        result.Timestamp.Should().Be(CurrentTime);
    }
}
