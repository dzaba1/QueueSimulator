using AutoFixture;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Dzaba.TestUtils;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Dzaba.QueueSimulator.Lib.Tests;

[TestFixture]
public class AgentsRepositoryTests
{
    private static readonly DateTime CurrentTime = DateTime.Now;

    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private AgentsRepository CreateSut()
    {
        return fixture.Create<AgentsRepository>();
    }

    private SimulationSettings GetSomeSettings()
    {
        return new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1"
                },
                new AgentConfiguration
                {
                    Name = "TestAgent2"
                },
                new AgentConfiguration
                {
                    Name = "TestAgent3"
                },
                new AgentConfiguration
                {
                    Name = "TestAgent4",
                    MaxInstances = 1
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"]
                },
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "BuildConfig1"
                }
            ]
        };
    }

    private void SetupSettings(SimulationSettings settings)
    {
        var context = fixture.FreezeMock<ISimulationContext>();
        var payload = new SimulationPayload(settings);
        context.Setup(x => x.Payload).Returns(payload);
    }

    [Test]
    public void EnumerateRequests_WhenCalled_ThenItReturnsAllAgents()
    {
        var settings = GetSomeSettings();
        SetupSettings(settings);
        var sut = CreateSut();

        for (var i = 0; i < 3; i++)
        {
            sut.TryCreateAgent(["TestAgent1"], CurrentTime, out _).Should().BeTrue();
        }

        sut.EnumerateAgents().Should().HaveCount(3);
    }

    [Test]
    public void GetAgent_WhenAgentCreated_ThenItCanBeReturnedById()
    {
        var settings = GetSomeSettings();
        SetupSettings(settings);
        var sut = CreateSut();

        sut.TryCreateAgent(["TestAgent1"], CurrentTime, out var agent).Should().BeTrue();

        var result = sut.GetAgent(agent.Id);
        result.Should().BeSameAs(agent);
    }

    [Test]
    public void GetActiveAgentsByConfigurationCount_WhenAgentsCreated_ThenItReturnsActiveOnes()
    {
        var settings = GetSomeSettings();
        SetupSettings(settings);
        var sut = CreateSut();

        var agentsList = new List<Agent>();

        for (var i = 0; i < 2; i++)
        {
            sut.TryCreateAgent(["TestAgent1"], CurrentTime, out var agent).Should().BeTrue();
            agentsList.Add(agent);

            sut.TryCreateAgent(["TestAgent2"], CurrentTime, out agent).Should().BeTrue();
            agentsList.Add(agent);

            sut.TryCreateAgent(["TestAgent3"], CurrentTime, out agent).Should().BeTrue();
            agentsList.Add(agent);
        }

        agentsList[0].State = AgentState.Finished;
        agentsList[1].State = AgentState.Finished;
        agentsList[2].State = AgentState.Finished;
        agentsList[5].State = AgentState.Finished;

        var result = sut.GetActiveAgentsByConfigurationCount();
        result.Should().HaveCount(2);
        result["TestAgent1"].Should().Be(1);
        result["TestAgent2"].Should().Be(1);
    }

    [Test]
    public void GetActiveAgentsCount_WhenAgentsCreated_ThenItReturnsActiveOnes()
    {
        var settings = GetSomeSettings();
        SetupSettings(settings);
        var sut = CreateSut();

        var agentsList = new List<Agent>();

        for (var i = 0; i < 2; i++)
        {
            sut.TryCreateAgent(["TestAgent1"], CurrentTime, out var agent).Should().BeTrue();
            agentsList.Add(agent);

            sut.TryCreateAgent(["TestAgent2"], CurrentTime, out agent).Should().BeTrue();
            agentsList.Add(agent);

            sut.TryCreateAgent(["TestAgent3"], CurrentTime, out agent).Should().BeTrue();
            agentsList.Add(agent);
        }

        agentsList[0].State = AgentState.Finished;
        agentsList[1].State = AgentState.Finished;
        agentsList[2].State = AgentState.Finished;
        agentsList[5].State = AgentState.Finished;

        var result = sut.GetActiveAgentsCount();
        result.Should().Be(2);
    }

    [Test]
    public void TryCreateAgent_WhenTotalMaxAgentsReached_ThenDontCreateANewOne()
    {
        var settings = GetSomeSettings();
        settings.MaxRunningAgents = 1;

        SetupSettings(settings);
        var sut = CreateSut();

        sut.TryCreateAgent(["TestAgent1"], CurrentTime, out _).Should().BeTrue();

        sut.TryCreateAgent(["TestAgent1"], CurrentTime, out var result).Should().BeFalse();
        result.Should().BeNull();
    }

    [Test]
    public void TryCreateAgent_WhenMaxAgentsReached_ThenDontCreateANewOne()
    {
        var settings = GetSomeSettings();
        SetupSettings(settings);
        var sut = CreateSut();

        sut.TryCreateAgent(["TestAgent4"], CurrentTime, out _).Should().BeTrue();

        sut.TryCreateAgent(["TestAgent4"], CurrentTime, out var result).Should().BeFalse();
        result.Should().BeNull();
    }

    [Test]
    public void TryCreateAgent_WhenMultipleCompatibleAgent_ThenSelectMinCountConfiguration()
    {
        var settings = GetSomeSettings();
        SetupSettings(settings);
        var sut = CreateSut();

        sut.TryCreateAgent(["TestAgent4"], CurrentTime, out _).Should().BeTrue();
        for (var i = 0; i < 3; i++)
        {
            sut.TryCreateAgent(["TestAgent1"], CurrentTime, out _).Should().BeTrue();
            sut.TryCreateAgent(["TestAgent2"], CurrentTime, out _).Should().BeTrue();
        }
        sut.TryCreateAgent(["TestAgent1"], CurrentTime, out _).Should().BeTrue();

        sut.TryCreateAgent(["TestAgent1", "TestAgent2", "TestAgent4"], CurrentTime, out var result).Should().BeTrue();
        result.AgentConfiguration.Should().Be("TestAgent2");
    }
}
