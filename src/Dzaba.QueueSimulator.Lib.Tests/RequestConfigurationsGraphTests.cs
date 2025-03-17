using Dzaba.QueueSimulator.Lib.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Dzaba.QueueSimulator.Lib.Tests;

[TestFixture]
public class RequestConfigurationsGraphTests
{
    [Test]
    public void AA()
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

        var sut = new RequestConfigurationsGraph(payload, settings.RequestConfigurations[0]);

        sut.GetChildren(settings.RequestConfigurations[0]).Should().HaveCount(1);
        sut.GetChildren(settings.RequestConfigurations[1]).Should().HaveCount(2);
        sut.GetChildren(settings.RequestConfigurations[2]).Should().HaveCount(1);
        sut.GetChildren(settings.RequestConfigurations[3]).Should().BeEmpty();
        sut.GetChildren(settings.RequestConfigurations[4]).Should().HaveCount(1);

        sut.GetParents(settings.RequestConfigurations[0]).Should().BeEmpty();
        sut.GetParents(settings.RequestConfigurations[1]).Should().HaveCount(1);
        sut.GetParents(settings.RequestConfigurations[2]).Should().HaveCount(1);
        sut.GetParents(settings.RequestConfigurations[3]).Should().HaveCount(2);
        sut.GetParents(settings.RequestConfigurations[4]).Should().HaveCount(1);
    }
}
