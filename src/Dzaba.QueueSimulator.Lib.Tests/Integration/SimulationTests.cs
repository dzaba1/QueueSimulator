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

    private TimeEventData ValidateLastToBeCompleted(TimeEventData[] events)
    {
        var last = events[events.Length - 1];
        ValidateDataForEmptines(last.RequestsQueue);
        ValidateDataForEmptines(last.RunningAgents);
        ValidateDataForEmptines(last.RunningRequests);
        last.Name.Should().Be(EventNames.FinishRequest);
        last.AllAgents.Should().AllSatisfy(a => a.State.Should().Be(AgentState.Finished));
        last.AllRequests.Should().AllSatisfy(r => r.State.Should().Be(RequestState.Finished));
        return last;
    }

    [Test]
    public void Run_WhenOneRequest_Then5Events()
    {
        var settings = new SimulationSettings
        {
            IncludeAllAgents = true,
            IncludeAllRequests = true,
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
        ValidateLastToBeCompleted(result);

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

        result[4].Timestamp.Minute.Should().Be(1);
    }

    [Test]
    public void Run_WhenOneRequestWithAgentInit_Then5Events()
    {
        var settings = new SimulationSettings
        {
            IncludeAllAgents = true,
            IncludeAllRequests = true,
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
        ValidateLastToBeCompleted(result);

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

        result[4].Timestamp.Minute.Should().Be(16);
    }

    [Test]
    public void Run_WhenALotOfQueuedRequests_ThenEveryoneIsProcessed()
    {
        var settings = new SimulationSettings
        {
            IncludeAllAgents = true,
            IncludeAllRequests = true,
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
        ValidateLastToBeCompleted(result);
    }

    [Test]
    public void Run_WhenOneRequestWithDependency_Then10Events()
    {
        var settings = new SimulationSettings
        {
            IncludeAllAgents = true,
            IncludeAllRequests = true,
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
        var last = ValidateLastToBeCompleted(result);

        result.Should().HaveCount(10);

        last.Timestamp.Minute.Should().Be(32);
        last.AllAgents.Should().HaveCount(2);
        last.AllRequests.Should().HaveCount(2);
        last.AllRequests[0].Id.Should().Be(1);
        last.AllRequests[0].Dependencies.Should().BeEquivalentTo([2]);
        last.AllRequests[1].Id.Should().Be(2);
    }

    [Test]
    public void Run_When4DiamondConfiguration_Then4Builds()
    {
        var settings = new SimulationSettings
        {
            IncludeAllAgents = true,
            IncludeAllRequests = true,
            Agents = [
                new AgentConfiguration
                {
                    Name = "TestAgent1",
                    InitTime = TimeSpan.FromMinutes(1)
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig2", "BuildConfig3"],
                    Duration = TimeSpan.FromMinutes(1)
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig2",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig4"],
                    Duration = TimeSpan.FromMinutes(1)
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig3",
                    CompatibleAgents = ["TestAgent1"],
                    RequestDependencies = ["BuildConfig4"],
                    Duration = TimeSpan.FromMinutes(1)
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig4",
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
        var last = ValidateLastToBeCompleted(result);

        last.Timestamp.Minute.Should().Be(6);
        last.AllAgents.Should().HaveCount(4);
        last.AllRequests.Should().HaveCount(4);
        last.AllRequests[0].Dependencies.Should().BeEquivalentTo([2,3]);
        last.AllRequests[1].Dependencies.Should().BeEquivalentTo([4]);
        last.AllRequests[2].Dependencies.Should().BeEquivalentTo([4]);
    }
}
