using Dzaba.QueueSimulator.Lib.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Dzaba.QueueSimulator.Lib.Tests;

[TestFixture]
public class PipelineTests
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
    public void SetReference_WhenReference_ThenYouCanGetParents()
    {
        var payload = GetSomePayload();

        var request1 = new Request
        {
            Id = 1,
            RequestConfiguration = "BuildConfig1"
        };
        var request2 = new Request
        {
            Id = 2,
            RequestConfiguration = "BuildConfig2"
        };

        var sut = new Pipeline(payload.SimulationSettings.RequestConfigurations[0], payload);

        sut.SetReference(request2, request1);

        sut.GetParents(request2, false).Should().HaveCount(1);
        sut.GetParents(request1, false).Should().BeEmpty();
    }

    [Test]
    public void SetReference_WhenReference_ThenYouCanGetChildren()
    {
        var payload = GetSomePayload();

        var request1 = new Request
        {
            Id = 1,
            RequestConfiguration = "BuildConfig1"
        };
        var request2 = new Request
        {
            Id = 2,
            RequestConfiguration = "BuildConfig2"
        };

        var sut = new Pipeline(payload.SimulationSettings.RequestConfigurations[0], payload);

        sut.SetReference(request2, request1);

        sut.GetChildren(request1).Should().HaveCount(1);
        sut.GetChildren(request2).Should().BeEmpty();
    }

    [Test]
    public void SetReference_WhenReference_ThenYouCanGetParentsRecurse()
    {
        var payload = GetSomePayload();

        var request1 = new Request
        {
            Id = 1,
            RequestConfiguration = "BuildConfig1"
        };
        var request2 = new Request
        {
            Id = 2,
            RequestConfiguration = "BuildConfig2"
        };
        var request3 = new Request
        {
            Id = 3,
            RequestConfiguration = "BuildConfig3"
        };

        var sut = new Pipeline(payload.SimulationSettings.RequestConfigurations[0], payload);

        sut.SetReference(request2, request1);
        sut.SetReference(request3, request2);

        sut.GetParents(request3, true).Should().HaveCount(2);
    }
}
