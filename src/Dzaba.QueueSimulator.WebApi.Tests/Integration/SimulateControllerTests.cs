using Dzaba.QueueSimulator.Lib.Model;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Dzaba.QueueSimulator.WebApi.Tests.Integration;

[TestFixture]
public class SimulateControllerTests : ControllerTestFixture
{
    [Test]
    public async Task Csv_WhenSomeModel_ThenCsv()
    {
        var client = CreateClient();

        var settings = new SimulationSettings
        {
            IncludeAllAgents = true,
            IncludeAllRequests = true,
            Agents = [
                new AgentConfiguration
                {
                    Name = "Agent1"
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
                    Duration = TimeSpan.FromMinutes(3),
                },
                new RequestConfiguration
                {
                    Name = "Tests",
                    CompatibleAgents = ["Agent1"],
                    Duration = TimeSpan.FromMinutes(1),
                    RequestDependencies = ["Build"]
                },
                new RequestConfiguration
                {
                    Name = "Publish",
                    CompatibleAgents = ["Agent1"],
                    Duration = TimeSpan.FromMinutes(1),
                    RequestDependencies = ["Tests"]
                }
            ],
            InitialRequests = [
                new InitialRequest
                {
                    Name = "Full pipeline",
                    NumberToQueue = 20
                }
            ]
        };

        using var body = SerializeJsonBody(settings);

        using var resp = await client.PostAsync("/simulate/csv", body);
        var result = await ReadFullStringAsync(resp);
        result.Should().StartWith("Timestamp,Name,Message,TotalRunningAgents,TotalRunningRequests,TotalRequestsQueue,RunningAgent_Agent1,RunningRequests_Full_pipeline,RequestsQueue_Full_pipeline,RunningRequests_Build,RequestsQueue_Build,RunningRequests_Tests,RequestsQueue_Tests,RunningRequests_Publish,RequestsQueue_Publish");
        result.Should().EndWith("\"01/01/2025 07:41:00\",\"FinishRequest\",\"Finished the request 77 [Full pipeline].\",0,0,0,0,0,0,0,0,0,0,0,0");
    }
}
