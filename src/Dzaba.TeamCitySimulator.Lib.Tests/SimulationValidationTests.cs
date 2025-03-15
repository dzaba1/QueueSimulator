﻿using AutoFixture;
using Dzaba.TeamCitySimulator.Lib.Model;
using Dzaba.TestUtils;
using FluentAssertions;
using NUnit.Framework;

namespace Dzaba.TeamCitySimulator.Lib.Tests;

[TestFixture]
public class SimulationValidationTests
{
    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private SimulationValidation CreateSut()
    {
        return fixture.Create<SimulationValidation>();
    }

    [Test]
    public void Validate_WhenBuildConfigWithWrongAgent_ThenError()
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
                    CompatibleAgents = ["TestAgent2"]
                }
            ],
            QueuedBuilds = []
        };
        var payload = new SimulationPayload(settings);

        var sut = CreateSut();

        this.Invoking(_ => sut.Validate(payload))
            .Should().Throw<ExitCodeException>().Which.ExitCode.Should().Be(ExitCode.BuildAgentNotFound);
    }

    [Test]
    public void Validate_WhenBuildConfigNotFoundInDependencies_ThenError()
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
                    BuildDependencies = ["BuildConfig2"]
                }
            ],
            QueuedBuilds = []
        };
        var payload = new SimulationPayload(settings);

        var sut = CreateSut();

        this.Invoking(_ => sut.Validate(payload))
            .Should().Throw<ExitCodeException>().Which.ExitCode.Should().Be(ExitCode.BuildNotFound);
    }

    [Test]
    public void Validate_WhenBuildConfigNotFoundInQueuedBuilds_ThenError()
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
                    CompatibleAgents = ["TestAgent1"]
                }
            ],
            QueuedBuilds = [
                new QueuedBuild
                {
                    Name = "BuildConfig2"
                }
            ]
        };
        var payload = new SimulationPayload(settings);

        var sut = CreateSut();

        this.Invoking(_ => sut.Validate(payload))
            .Should().Throw<ExitCodeException>().Which.ExitCode.Should().Be(ExitCode.BuildNotFound);
    }

    [Test]
    public void Validate_WhenBuildCyclicDependency_ThenError()
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
                    BuildDependencies = ["BuildConfig1"]
                }
            ],
            QueuedBuilds = [
                new QueuedBuild
                {
                    Name = "BuildConfig1"
                }
            ]
        };
        var payload = new SimulationPayload(settings);

        var sut = CreateSut();

        this.Invoking(_ => sut.Validate(payload))
            .Should().Throw<ExitCodeException>().Which.ExitCode.Should().Be(ExitCode.BuildCyclicDependency);
    }

    [Test]
    public void Validate_WhenBuild3CyclicDependency_ThenError()
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
                    BuildDependencies = ["BuildConfig2"]
                },
                new BuildConfiguration
                {
                    Name = "BuildConfig2",
                    CompatibleAgents = ["TestAgent1"],
                    BuildDependencies = ["BuildConfig3"]
                },
                new BuildConfiguration
                {
                    Name = "BuildConfig3",
                    CompatibleAgents = ["TestAgent1"],
                    BuildDependencies = ["BuildConfig1"]
                }
            ],
            QueuedBuilds = [
                new QueuedBuild
                {
                    Name = "BuildConfig1"
                }
            ]
        };
        var payload = new SimulationPayload(settings);

        var sut = CreateSut();

        this.Invoking(_ => sut.Validate(payload))
            .Should().Throw<ExitCodeException>().Which.ExitCode.Should().Be(ExitCode.BuildCyclicDependency);
    }

    [Test]
    public void Validate_WhenBuildsOk_ThenNoErrors()
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
                    BuildDependencies = ["BuildConfig2"]
                },
                new BuildConfiguration
                {
                    Name = "BuildConfig2",
                    CompatibleAgents = ["TestAgent1"],
                    BuildDependencies = ["BuildConfig3", "BuildConfig5"]
                },
                new BuildConfiguration
                {
                    Name = "BuildConfig3",
                    CompatibleAgents = ["TestAgent1"],
                    BuildDependencies = ["BuildConfig4"]
                },
                new BuildConfiguration
                {
                    Name = "BuildConfig4",
                    CompatibleAgents = ["TestAgent1"]
                },
                new BuildConfiguration
                {
                    Name = "BuildConfig5",
                    CompatibleAgents = ["TestAgent1"],
                    BuildDependencies = ["BuildConfig4"]
                }
            ],
            QueuedBuilds = [
                new QueuedBuild
                {
                    Name = "BuildConfig1"
                }
            ]
        };
        var payload = new SimulationPayload(settings);

        var sut = CreateSut();

        sut.Validate(payload);
    }
}
