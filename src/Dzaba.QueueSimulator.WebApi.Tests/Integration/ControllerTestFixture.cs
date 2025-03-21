using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dzaba.QueueSimulator.WebApi.Tests.Integration;

public class ControllerTestFixture
{
    private WebApplicationFactory<Program> factory;

    [SetUp]
    public void SetupFactory()
    {
        factory = new WebApplicationFactory<Program>();
    }

    protected HttpClient CreateClient()
    {
        return factory.CreateClient();
    }

    protected StringContent SerializeJsonBody(object obj)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        string json = JsonSerializer.Serialize(obj, jsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    protected async Task<string> ReadFullStringAsync(HttpResponseMessage resp)
    {
        var str = await resp.Content.ReadAsStringAsync();
        this.Invoking(_ => resp.EnsureSuccessStatusCode())
            .Should().NotThrow(str);
        return str;
    }
}
