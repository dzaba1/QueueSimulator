using Dzaba.QueueSimulator.Lib.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Dzaba.QueueSimulator.Lib.Tests;

[TestFixture]
public class RequestConfigurationsGraphTests
{
    private SimulationPayload GetSomePayload()
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
        return new SimulationPayload(settings);
    }

    [Test]
    public void GetChildren_WhenSomeGraph_ThenChildrenCountIsOk()
    {
        var payload = GetSomePayload();

        var sut = new RequestConfigurationsGraph(payload, payload.SimulationSettings.RequestConfigurations[0]);

        sut.GetChildren(payload.SimulationSettings.RequestConfigurations[0]).Should().HaveCount(1);
        sut.GetChildren(payload.SimulationSettings.RequestConfigurations[1]).Should().HaveCount(2);
        sut.GetChildren(payload.SimulationSettings.RequestConfigurations[2]).Should().HaveCount(1);
        sut.GetChildren(payload.SimulationSettings.RequestConfigurations[3]).Should().BeEmpty();
        sut.GetChildren(payload.SimulationSettings.RequestConfigurations[4]).Should().HaveCount(1);
    }

    [Test]
    public void GetParents_WhenSomeGraph_ThenChildrenCountIsOk()
    {
        var payload = GetSomePayload();

        var sut = new RequestConfigurationsGraph(payload, payload.SimulationSettings.RequestConfigurations[0]);

        sut.GetParents(payload.SimulationSettings.RequestConfigurations[0]).Should().BeEmpty();
        sut.GetParents(payload.SimulationSettings.RequestConfigurations[1]).Should().HaveCount(1);
        sut.GetParents(payload.SimulationSettings.RequestConfigurations[2]).Should().HaveCount(1);
        sut.GetParents(payload.SimulationSettings.RequestConfigurations[3]).Should().HaveCount(2);
        sut.GetParents(payload.SimulationSettings.RequestConfigurations[4]).Should().HaveCount(1);
    }
}
