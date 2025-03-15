using AutoFixture;
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
    public void Run_WhenOneBuild_ThenTwoEvents()
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
        result.Should().HaveCount(2);
    }
}
