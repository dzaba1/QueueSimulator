using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.TestUtils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Tests.Integration;

[TestFixture]
public class SimulationTests : IocTestFixture
{
    private ISimulation CreateSut()
    {
        return Container.GetRequiredService<ISimulation>();
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
    public void Run_WhenOneRequest_Then4Events()
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
                    Duration = TimeSpan.FromMinutes(1)
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "BuildConfig1",
                    NumberToQueue = 1
                }
            ]
        };

        var sut = CreateSut();

        var result = sut.Run(settings).ToArray();
        result.Should().HaveCount(4);

        result[0].Name.Should().Be(EventNames.QueueRequest);
        ValidateDataForCount(result[0].RequestsQueue, 1);
        ValidateDataForEmptines(result[0].RunningAgents);
        ValidateDataForEmptines(result[0].RunningRequests);

        result[1].Name.Should().Be(EventNames.CreateAgent);
        ValidateDataForCount(result[1].RequestsQueue, 1);
        ValidateDataForCount(result[1].RunningAgents, 1);
        ValidateDataForEmptines(result[1].RunningRequests);

        result[2].Name.Should().Be(EventNames.StartRequest);
        ValidateDataForEmptines(result[2].RequestsQueue);
        ValidateDataForCount(result[2].RunningAgents, 1);
        ValidateDataForCount(result[2].RunningRequests, 1);

        result[3].Name.Should().Be(EventNames.FinishRequest);
        ValidateDataForEmptines(result[3].RequestsQueue);
        ValidateDataForEmptines(result[3].RunningAgents);
        ValidateDataForEmptines(result[3].RunningRequests);
        result[3].Timestamp.Minute.Should().Be(1);
    }

    [Test]
    public void Run_WhenOneRequestWithAgentInit_Then5Events()
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
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    Duration = TimeSpan.FromMinutes(1)
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "BuildConfig1",
                    NumberToQueue = 1
                }
            ]
        };

        var sut = CreateSut();

        var result = sut.Run(settings).ToArray();
        result.Should().HaveCount(5);

        result[0].Name.Should().Be(EventNames.QueueRequest);
        ValidateDataForCount(result[0].RequestsQueue, 1);
        ValidateDataForEmptines(result[0].RunningAgents);
        ValidateDataForEmptines(result[0].RunningRequests);

        result[1].Name.Should().Be(EventNames.CreateAgent);
        ValidateDataForCount(result[1].RequestsQueue, 1);
        ValidateDataForCount(result[1].RunningAgents, 1);
        ValidateDataForEmptines(result[1].RunningRequests);

        result[2].Name.Should().Be(EventNames.InitAgent);
        ValidateDataForCount(result[2].RequestsQueue, 1);
        ValidateDataForCount(result[2].RunningAgents, 1);
        ValidateDataForEmptines(result[2].RunningRequests);

        result[3].Name.Should().Be(EventNames.StartRequest);
        ValidateDataForEmptines(result[3].RequestsQueue);
        ValidateDataForCount(result[3].RunningAgents, 1);
        ValidateDataForCount(result[3].RunningRequests, 1);

        result[4].Name.Should().Be(EventNames.FinishRequest);
        ValidateDataForEmptines(result[4].RequestsQueue);
        ValidateDataForEmptines(result[4].RunningAgents);
        ValidateDataForEmptines(result[4].RunningRequests);
        result[4].Timestamp.Minute.Should().Be(16);
    }

    [Test]
    public void Run_WhenALotOfQueuedRequests_ThenEveryoneIsProcessed()
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
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    Duration = TimeSpan.FromHours(1)
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "BuildConfig1",
                    NumberToQueue = 130
                }
            ]
        };

        var sut = CreateSut();

        var result = sut.Run(settings).ToArray();
        var last = result[result.Length - 1];
        ValidateDataForEmptines(last.RequestsQueue);
        ValidateDataForEmptines(last.RunningAgents);
        ValidateDataForEmptines(last.RunningRequests);
    }

    [Test]
    public void Run_WhenOneRequestWithDependency_Then4Events()
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
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    Duration = TimeSpan.FromMinutes(1)
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig2",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig1"],
                    Duration = TimeSpan.FromMinutes(1)
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "BuildConfig2",
                    NumberToQueue = 1
                }
            ]
        };

        var sut = CreateSut();

        var result = sut.Run(settings).ToArray();
        result.Should().HaveCount(1);
    }
}
