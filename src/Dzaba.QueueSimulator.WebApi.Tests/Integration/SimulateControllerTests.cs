using NUnit.Framework;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dzaba.QueueSimulator.WebApi.Tests.Integration;

[TestFixture]
public class SimulateControllerTests : ControllerTestFixture
{
    [Test]
    public async Task Csv_WhenSomeModel_ThenCsv()
    {
        var client = CreateClient();

        var json = @"{
}";

        using var body = new StringContent(json, Encoding.UTF8, "application/json");

        using var resp = await client.PostAsync("/simulate/csv", body);
        resp.EnsureSuccessStatusCode();

        var result = await resp.Content.ReadAsStringAsync();
    }
}
