using AutoFixture;
using Dzaba.TeamCitySimulator.Lib.Events;
using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TestUtils;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib.Tests;

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

    [Test]
    public void Run_WhenOneBuild_Then4Events()
    {
        var settings = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1"
                }
            ],
            BuildConfigurations = [
                new BuildConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    Duration = TimeSpan.FromMinutes(1)
                }
            ],
            QueuedBuilds = [
                new QueuedBuild
                {
                    Name = "BuildConfig1",
                    BuildsToQueue = 1
                }
            ]
        };

        var sut = CreateSut();

        var result = sut.Run(settings).ToArray();
        result.Should().HaveCount(4);

        result[0].Name.Should().Be(EventNames.QueueBuild);
        result[0].BuildConfigurationQueues.Should().HaveCount(1);
        result[0].QueueLength.Should().Be(1);
        result[0].RunningAgents.Should().BeEmpty();
        result[0].RunningBuilds.Should().BeEmpty();
        result[0].TotalRunningBuilds.Should().Be(0);

        result[1].Name.Should().Be(EventNames.CreateAgent);
        result[1].BuildConfigurationQueues.Should().HaveCount(1);
        result[1].QueueLength.Should().Be(1);
        result[1].RunningAgents.Should().HaveCount(1);
        result[1].RunningBuilds.Should().BeEmpty();
        result[1].TotalRunningBuilds.Should().Be(0);

        result[2].Name.Should().Be(EventNames.StartBuild);
        result[2].BuildConfigurationQueues.Should().BeEmpty();
        result[2].QueueLength.Should().Be(0);
        result[2].RunningAgents.Should().HaveCount(1);
        result[2].RunningBuilds.Should().HaveCount(1);
        result[2].TotalRunningBuilds.Should().Be(1);

        result[3].Name.Should().Be(EventNames.FinishBuild);
        result[3].BuildConfigurationQueues.Should().BeEmpty();
        result[3].QueueLength.Should().Be(0);
        result[3].RunningAgents.Should().BeEmpty();
        result[3].RunningBuilds.Should().BeEmpty();
        result[3].TotalRunningBuilds.Should().Be(0);
        result[3].Timestamp.Minute.Should().Be(1);
    }

    [Test]
    public void Run_WhenOneBuildWithAgentInit_Then4Events()
    {
        var settings = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1",
                    InitTime = TimeSpan.FromMinutes(15)
                }
            ],
            BuildConfigurations = [
                new BuildConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    Duration = TimeSpan.FromMinutes(1)
                }
            ],
            QueuedBuilds = [
                new QueuedBuild
                {
                    Name = "BuildConfig1",
                    BuildsToQueue = 1
                }
            ]
        };

        var sut = CreateSut();

        var result = sut.Run(settings).ToArray();
        result.Should().HaveCount(5);

        result[0].Name.Should().Be(EventNames.QueueBuild);
        result[0].BuildConfigurationQueues.Should().HaveCount(1);
        result[0].QueueLength.Should().Be(1);
        result[0].RunningAgents.Should().BeEmpty();
        result[0].RunningBuilds.Should().BeEmpty();
        result[0].TotalRunningBuilds.Should().Be(0);

        result[1].Name.Should().Be(EventNames.CreateAgent);
        result[1].BuildConfigurationQueues.Should().HaveCount(1);
        result[1].QueueLength.Should().Be(1);
        result[1].RunningAgents.Should().HaveCount(1);
        result[1].RunningBuilds.Should().BeEmpty();
        result[1].TotalRunningBuilds.Should().Be(0);

        result[2].Name.Should().Be(EventNames.InitAgent);
        result[2].BuildConfigurationQueues.Should().HaveCount(1);
        result[2].QueueLength.Should().Be(1);
        result[2].RunningAgents.Should().HaveCount(1);
        result[2].RunningBuilds.Should().BeEmpty();
        result[2].TotalRunningBuilds.Should().Be(0);

        result[3].Name.Should().Be(EventNames.StartBuild);
        result[3].BuildConfigurationQueues.Should().BeEmpty();
        result[3].QueueLength.Should().Be(0);
        result[3].RunningAgents.Should().HaveCount(1);
        result[3].RunningBuilds.Should().HaveCount(1);
        result[3].TotalRunningBuilds.Should().Be(1);

        result[4].Name.Should().Be(EventNames.FinishBuild);
        result[4].BuildConfigurationQueues.Should().BeEmpty();
        result[4].QueueLength.Should().Be(0);
        result[4].RunningAgents.Should().BeEmpty();
        result[4].RunningBuilds.Should().BeEmpty();
        result[4].TotalRunningBuilds.Should().Be(0);
        result[4].Timestamp.Minute.Should().Be(16);
    }
}
