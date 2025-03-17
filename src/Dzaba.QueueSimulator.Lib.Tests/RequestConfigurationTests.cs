using Dzaba.QueueSimulator.Lib.Model;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Tests;

[TestFixture]
public class RequestConfigurationTests
{
    private SimulationSettings GetSomeSettings()
    {
        return new SimulationSettings
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
    }

    [Test]
    public void ResolveRequestConfigurationDependencies_WhenCalledWithoutRecursion_ThenItReturnsDistinctValues()
    {
        var settings = GetSomeSettings();
        var payload = new SimulationPayload(settings);
        var sut = settings.RequestConfigurations[1];

        var result = sut.ResolveDependencies(payload, false).ToArray();

        result.Should().NotContain(settings.RequestConfigurations[1]);
        result.Should().OnlyHaveUniqueItems(s => s.Name);
        result.Should().HaveCount(2);
    }

    [Test]
    public void ResolveRequestConfigurationDependencies_WhenCalledWithRecursion_ThenItReturnsDistinctValues()
    {
        var settings = GetSomeSettings();
        var payload = new SimulationPayload(settings);
        var sut = settings.RequestConfigurations[0];

        var result = sut.ResolveDependencies(payload, true)
            .ToArray();

        result.Should().NotContain(settings.RequestConfigurations[0]);
        result.Should().OnlyHaveUniqueItems(s => s.Name);
        result.Should().HaveCount(4);
    }
}
