using AutoFixture;
using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TestUtils;
using FluentAssertions;
using NUnit.Framework;

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
    public void Run_WhenBuildConfigWithWrongAgent_ThenError()
    {
        var settings = new SimulationSettings
        {
            Agents = [
                new Agent
                {
                    Name = "TestAgent1"
                }
            ],
            BuildConfigurations = [
                new Build
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent2"]
                }
            ]
        };

        var sut = CreateSut();

        this.Invoking(_ => sut.Run(settings))
            .Should().Throw<ExitCodeException>().Which.ExitCode.Should().Be(ExitCode.BuildAgentNotFound);
    }
}
