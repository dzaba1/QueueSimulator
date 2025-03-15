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

    private void ValidateDataForEmptines(ElementsData actual)
    {
        actual.Total.Should().Be(0);
        actual.Grouped.Should().BeEmpty();
    }

    private void ValidateDataForCount(ElementsData actual, int count)
    {
        actual.Total.Should().Be(count);
        actual.Grouped.Should().HaveCount(count);
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
        ValidateDataForCount(result[0].BuildsQueue, 1);
        ValidateDataForEmptines(result[0].RunningAgents);
        ValidateDataForEmptines(result[0].RunningBuilds);

        result[1].Name.Should().Be(EventNames.CreateAgent);
        ValidateDataForCount(result[1].BuildsQueue, 1);
        ValidateDataForCount(result[1].RunningAgents, 1);
        ValidateDataForEmptines(result[1].RunningBuilds);

        result[2].Name.Should().Be(EventNames.StartBuild);
        ValidateDataForEmptines(result[2].BuildsQueue);
        ValidateDataForCount(result[2].RunningAgents, 1);
        ValidateDataForCount(result[2].RunningBuilds, 1);

        result[3].Name.Should().Be(EventNames.FinishBuild);
        ValidateDataForEmptines(result[3].BuildsQueue);
        ValidateDataForEmptines(result[3].RunningAgents);
        ValidateDataForEmptines(result[3].RunningBuilds);
        result[3].Timestamp.Minute.Should().Be(1);
    }

    [Test]
    public void Run_WhenOneBuildWithAgentInit_Then5Events()
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
        ValidateDataForCount(result[0].BuildsQueue, 1);
        ValidateDataForEmptines(result[0].RunningAgents);
        ValidateDataForEmptines(result[0].RunningBuilds);

        result[1].Name.Should().Be(EventNames.CreateAgent);
        ValidateDataForCount(result[1].BuildsQueue, 1);
        ValidateDataForCount(result[1].RunningAgents, 1);
        ValidateDataForEmptines(result[1].RunningBuilds);

        result[2].Name.Should().Be(EventNames.InitAgent);
        ValidateDataForCount(result[2].BuildsQueue, 1);
        ValidateDataForCount(result[2].RunningAgents, 1);
        ValidateDataForEmptines(result[2].RunningBuilds);

        result[3].Name.Should().Be(EventNames.StartBuild);
        ValidateDataForEmptines(result[3].BuildsQueue);
        ValidateDataForCount(result[3].RunningAgents, 1);
        ValidateDataForCount(result[3].RunningBuilds, 1);

        result[4].Name.Should().Be(EventNames.FinishBuild);
        ValidateDataForEmptines(result[4].BuildsQueue);
        ValidateDataForEmptines(result[4].RunningAgents);
        ValidateDataForEmptines(result[4].RunningBuilds);
        result[4].Timestamp.Minute.Should().Be(16);
    }

    [Test]
    public void Run_WhenALotOfQueuedBuilds_ThenEveryoneIsProcessed()
    {
        var settings = new SimulationSettings
        {
            MaxRunningAgents = 120,
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1",
                    InitTime = TimeSpan.FromMinutes(15),
                    MaxInstances = 20
                }
            ],
            BuildConfigurations = [
                new BuildConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    Duration = TimeSpan.FromHours(1)
                }
            ],
            QueuedBuilds = [
                new QueuedBuild
                {
                    Name = "BuildConfig1",
                    BuildsToQueue = 130
                }
            ]
        };

        var sut = CreateSut();

        var result = sut.Run(settings).ToArray();
        var last = result[result.Length - 1];
        ValidateDataForEmptines(last.BuildsQueue);
        ValidateDataForEmptines(last.RunningAgents);
        ValidateDataForEmptines(last.RunningBuilds);
    }

    [Test]
    public void Run_WhenOneBuildWithDependency_Then4Events()
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
                },
                new BuildConfiguration
                {
                    Name = "BuildConfig2",
                    CompatibleAgents = ["TestAgent1"],
                    BuildDependencies = ["BuildConfig1"],
                    Duration = TimeSpan.FromMinutes(1)
                }
            ],
            QueuedBuilds = [
                new QueuedBuild
                {
                    Name = "BuildConfig2",
                    BuildsToQueue = 1
                }
            ]
        };

        var sut = CreateSut();

        var result = sut.Run(settings).ToArray();
        result.Should().HaveCount(1);
    }
}
