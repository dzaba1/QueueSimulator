using AutoFixture;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.TestUtils;
using FluentAssertions;
using NUnit.Framework;

namespace Dzaba.QueueSimulator.Lib.Tests;

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
    public void Validate_WhenRequestConfigWithWrongAgent_ThenError()
    {
        var settings = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1"
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent2"]
                }
            ],
            InitialRequests = []
        };
        var payload = new SimulationPayload(settings);

        var sut = CreateSut();

        this.Invoking(_ => sut.Validate(payload))
            .Should().Throw<ExitCodeException>().Which.Errors.Should().HaveCount(1).And.Satisfy(e => e.Key == ExitCode.AgentNotFound);
    }

    [Test]
    public void Validate_WhenRequestConfigNotFoundInDependencies_ThenError()
    {
        var settings = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1"
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig2"]
                }
            ],
            InitialRequests = []
        };
        var payload = new SimulationPayload(settings);

        var sut = CreateSut();

        this.Invoking(_ => sut.Validate(payload))
            .Should().Throw<ExitCodeException>().Which.Errors.Should().HaveCount(1).And.Satisfy(e => e.Key == ExitCode.RequestNotFound);
    }

    [Test]
    public void Validate_WhenRequestConfigNotFoundInQueuedRequests_ThenError()
    {
        var settings = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1"
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"]
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "BuildConfig2"
                }
            ]
        };
        var payload = new SimulationPayload(settings);

        var sut = CreateSut();

        this.Invoking(_ => sut.Validate(payload))
            .Should().Throw<ExitCodeException>().Which.Errors.Should().HaveCount(1).And.Satisfy(e => e.Key == ExitCode.RequestNotFound);
    }

    [Test]
    public void Validate_WhenRequestCyclicDependency_ThenError()
    {
        var settings = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1"
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig1"]
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "BuildConfig1"
                }
            ]
        };
        var payload = new SimulationPayload(settings);

        var sut = CreateSut();

        this.Invoking(_ => sut.Validate(payload))
            .Should().Throw<ExitCodeException>().Which.Errors.Should().HaveCount(1).And.Satisfy(e => e.Key == ExitCode.RequestCyclicDependency);
    }

    [Test]
    public void Validate_WhenRequest3CyclicDependency_ThenError()
    {
        var settings = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1"
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig2"]
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig2",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig3"]
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig3",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig1"]
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "BuildConfig1"
                }
            ]
        };
        var payload = new SimulationPayload(settings);

        var sut = CreateSut();

        this.Invoking(_ => sut.Validate(payload))
            .Should().Throw<ExitCodeException>().Which.Errors.Should().HaveCount(1).And.Satisfy(e => e.Key == ExitCode.RequestCyclicDependency);
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
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig2"]
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig2",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig3", "BuildConfig5"]
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig3",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig4"]
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig4",
                    CompatibleAgents = ["TestAgent1"]
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig5",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig4"]
                }
            ],
            InitialRequests = [
                new InitialRequest
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
