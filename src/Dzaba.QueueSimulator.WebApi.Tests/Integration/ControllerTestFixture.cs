using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http;

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
}
