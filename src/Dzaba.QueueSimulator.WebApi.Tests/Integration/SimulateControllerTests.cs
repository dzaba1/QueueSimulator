using Dzaba.QueueSimulator.Lib.Model;
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
   
    }
}
