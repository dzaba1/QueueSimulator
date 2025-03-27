using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Model.Distribution;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Dzaba.QueueSimulator.WebApi.Tests.Integration;

[TestFixture]
public class SimulateControllerTests : ControllerTestFixture
{
    private void AssertCsvFieldsCount(string csv)
    {
        using var reader = new StringReader(csv);
        int? firstCount = null;

        var line = reader.ReadLine();
        if (line != null)
        {
            var count = line.Split(',').Length;
            if (firstCount == null)
            {
                firstCount = count;
            }
            else
            {
                count.Should().Be(firstCount.Value);
            }

            line = reader.ReadLine();
        }
    }

    [Test]
    public async Task Csv_WhenSomeModel_ThenCsv()
    {
        var client = CreateClient();

        var settings = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "Agent1",
                    InitTime = new StaticDuration
                    {
                        Value = TimeSpan.FromMinutes(1)
                    }
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "Full pipeline",
                    RequestDependencies = ["Build", "Tests", "Publish"],
                    IsComposite = true,
                },
                new RequestConfiguration
                {
                    Name = "Build",
                    CompatibleAgents = ["Agent1"],
                    Duration = new StaticDuration
                    {
                        Value = TimeSpan.FromMinutes(3)
                    },
                },
                new RequestConfiguration
                {
                    Name = "Tests",
                    CompatibleAgents = ["Agent1"],
                    Duration = new StaticDuration
                    {
                        Value = TimeSpan.FromMinutes(1)
                    },
                    RequestDependencies = ["Build"]
                },
                new RequestConfiguration
                {
                    Name = "Publish",
                    CompatibleAgents = ["Agent1"],
                    Duration = new StaticDuration
                    {
                        Value = TimeSpan.FromMinutes(1)
                    },
                    RequestDependencies = ["Tests"]
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "Full pipeline",
                    Distribution = new DurationInitialDistribution
                    {
                        Duration = new StaticDuration
                        {
                            Value = TimeSpan.FromHours(8)
                        },
                        NumberToQueue = 20
                    }
                }
            ],
            ReportSettings = new ReportSettings
            {
                IncludeAllAgents = true,
                IncludeAllRequests = true
            }
        };

        using var body = SerializeJsonBody(settings);

        using var resp = await client.PostAsync("/simulate/csv", body);
        var result = await ReadFullStringAsync(resp);
        AssertCsvFieldsCount(result);
        result.Should().StartWith("Timestamp,Name,Message,TotalRunningAgents,TotalRunningRequests,TotalRequestsQueue,AvgFinishedRequestDuration_Full_pipeline,RunningAgent_Agent1,RunningRequests_Full_pipeline,RequestsQueue_Full_pipeline,RunningRequests_Build,RequestsQueue_Build,RunningRequests_Tests,RequestsQueue_Tests,RunningRequests_Publish,RequestsQueue_Publish");
        result.Should().EndWith("\"01/01/2025 07:44:00\",\"FinishRequest\",\"Finished the request 77 [Full pipeline].\",0,0,0,\"00:07:00\",0,0,0,0,0,0,0,0,0");
    }

    [Test]
    public async Task Csv_WhenSomeModelWithFilters_ThenCsv()
    {
        var client = CreateClient();

        var settings = new SimulationSettings
        {
            Agents = [
                new AgentConfiguration
                {
                    Name = "Agent1",
                    InitTime = new StaticDuration
                    {
                        Value = TimeSpan.FromMinutes(1)
                    }
                },
                new AgentConfiguration
                {
                    Name = "Agent2",
                    InitTime = new StaticDuration
                    {
                        Value = TimeSpan.FromMinutes(1)
                    }
                }
            ],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "Full pipeline",
                    RequestDependencies = ["Build", "Tests", "Publish"],
                    IsComposite = true,
                },
                new RequestConfiguration
                {
                    Name = "Build",
                    CompatibleAgents = ["Agent1"],
                    Duration = new StaticDuration
                    {
                        Value = TimeSpan.FromMinutes(3)
                    },
                },
                new RequestConfiguration
                {
                    Name = "Tests",
                    CompatibleAgents = ["Agent1"],
                    Duration = new StaticDuration
                    {
                        Value = TimeSpan.FromMinutes(1)
                    },
                    RequestDependencies = ["Build"]
                },
                new RequestConfiguration
                {
                    Name = "Publish",
                    CompatibleAgents = ["Agent1"],
                    Duration = new StaticDuration
                    {
                        Value = TimeSpan.FromMinutes(1)
                    },
                    RequestDependencies = ["Tests"]
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "Full pipeline",
                    Distribution = new DurationInitialDistribution
                    {
                        Duration = new StaticDuration
                        {
                            Value = TimeSpan.FromHours(8)
                        },
                        NumberToQueue = 20
                    }
                }
            ],
            ReportSettings = new ReportSettings
            {
                IncludeAllAgents = true,
                IncludeAllRequests = true,
                RequestConfigurationsToObserve = ["Full pipeline"],
                AgentConfigurationsToObserve = ["Agent1"],
                CsvSaveTimestampTicks = true
            }
        };

        using var body = SerializeJsonBody(settings);

        using var resp = await client.PostAsync("/simulate/csv", body);
        var result = await ReadFullStringAsync(resp);
        AssertCsvFieldsCount(result);
        result.Should().StartWith("Timestamp_Ticks,Name,Message,TotalRunningAgents,TotalRunningRequests,TotalRequestsQueue,AvgFinishedRequestDuration_Full_pipeline,RunningAgent_Agent1,RunningRequests_Full_pipeline,RequestsQueue_Full_pipeline");
        result.Should().EndWith("638713142400000000,\"FinishRequest\",\"Finished the request 77 [Full pipeline].\",0,0,0,\"00:07:00\",0,0,0");
    }
}
