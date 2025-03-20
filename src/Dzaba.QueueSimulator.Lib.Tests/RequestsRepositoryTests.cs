using AutoFixture;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Dzaba.TestUtils;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Tests;

[TestFixture]
public class RequestsRepositoryTests
{
    private static readonly DateTime CurrentTime = DateTime.Now;

    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private RequestsRepository CreateSut()
    {
        return fixture.Create<RequestsRepository>();
    }

    private SimulationSettings GetSomeSettings()
    {
        return new SimulationSettings
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
    }

    [Test]
    public void GetRequest_WhenRequestAdded_ThenItCanBeTakenById()
    {
        var settings = GetSomeSettings();
        var sut = CreateSut();

        var request = sut.NewRequest(settings.RequestConfigurations[0], Mock.Of<IPipeline>(), CurrentTime);
        var result = sut.GetRequest(request.Id);
        result.Should().BeSameAs(request);
    }

    [Test]
    public void NewRequest_WhenRequestAdded_ThenItHasCorrectProperties()
    {
        var settings = GetSomeSettings();
        var sut = CreateSut();

        var request = sut.NewRequest(settings.RequestConfigurations[0], Mock.Of<IPipeline>(), CurrentTime);
        request.AgentId.Should().BeNull();
        request.RequestConfiguration.Should().Be(settings.RequestConfigurations[0].Name);
        request.CreatedTime.Should().Be(CurrentTime);
        request.EndTime.Should().BeNull();
        request.StartTime.Should().BeNull();
        request.Id.Should().Be(1);
        request.State.Should().Be(RequestState.Created);
    }

    [Test]
    public void EnumerateRequests_WhenCalled_ThenItReturnsAllRequests()
    {
        var settings = GetSomeSettings();
        var sut = CreateSut();

        for (var i = 0; i < 3; i++)
        {
            sut.NewRequest(settings.RequestConfigurations[i], Mock.Of<IPipeline>(), CurrentTime);
        }

        sut.EnumerateRequests().Should().HaveCount(3);
    }

    [Test]
    public void GetWaitingForDependencies_WhenCalled_ThenItReturnsAllRequestsWaitingForDependencies()
    {
        var settings = GetSomeSettings();
        var sut = CreateSut();

        for (var i = 0; i < 3; i++)
        {
            var request = sut.NewRequest(settings.RequestConfigurations[i], Mock.Of<IPipeline>(), CurrentTime);
            if (i > 0)
            {
                request.State = RequestState.WaitingForDependencies;
            }
        }

        sut.GetWaitingForDependencies().Should().HaveCount(2);
    }

    [Test]
    public void GetWaitingForAgents_WhenCalled_ThenItReturnsAllRequestWaitingForAgents()
    {
        var settings = GetSomeSettings();
        var sut = CreateSut();

        var requests = Enumerable.Range(0, 6)
            .Select(i => sut.NewRequest(settings.RequestConfigurations[0], Mock.Of<IPipeline>(), CurrentTime))
            .ToArray();

        for (var i = 0; i < 4; i++)
        {
            if (i > 1)
            {
                requests[i].AgentId = i;
            }

            requests[i].State = RequestState.WaitingForAgent;
        }

        sut.GetWaitingForAgents().Should().HaveCount(4);
    }

    [Test]
    public void GroupRunningRequestsByConfiguration_WhenCalled_ThenItReturnsAllRequests()
    {
        var settings = GetSomeSettings();
        var sut = CreateSut();

        for (var i = 0; i < 3; i++)
        {
            var request = sut.NewRequest(settings.RequestConfigurations[i], Mock.Of<IPipeline>(), CurrentTime);
            request.State = RequestState.Running;
        }

        var result = sut.GroupRunningRequestsByConfiguration();
        result.Should().HaveCount(3);
        for (var i = 0; i < 3; i++)
        {
            result[settings.RequestConfigurations[i].Name].Should().HaveCount(1);
        }
    }

    [Test]
    public void GetRunningRequestCount_WhenCalled_ThenItCountsAllRunningRequests()
    {
        var settings = GetSomeSettings();
        var sut = CreateSut();

        for (var i = 0; i < 3; i++)
        {
            var request = sut.NewRequest(settings.RequestConfigurations[i], Mock.Of<IPipeline>(), CurrentTime);
            request.State = RequestState.Running;
        }

        var result = sut.GetRunningRequestCount();
        result.Should().Be(3);
    }

    [Test]
    public void GroupQueueByConfiguration_WhenCalled_ThenItReturnsAllRequests()
    {
        var settings = GetSomeSettings();
        var sut = CreateSut();

        for (var i = 0; i < 3; i++)
        {
            sut.NewRequest(settings.RequestConfigurations[i], Mock.Of<IPipeline>(), CurrentTime);
        }

        var result = sut.GroupQueueByConfiguration();
        result.Should().HaveCount(3);
        for (var i = 0; i < 3; i++)
        {
            result[settings.RequestConfigurations[i].Name].Should().HaveCount(1);
        }
    }

    [Test]
    public void GetQueueLengtht_WhenCalled_ThenItCountsAllQueuedRequests()
    {
        var settings = GetSomeSettings();
        var sut = CreateSut();

        for (var i = 0; i < 3; i++)
        {
            sut.NewRequest(settings.RequestConfigurations[i], Mock.Of<IPipeline>(), CurrentTime);
        }

        var result = sut.GetQueueLength();
        result.Should().Be(3);
    }

    [Test]
    public void GetPipeline_WhenRequestAdded_ThenPipelineForItCanBeTaken()
    {
        var settings = GetSomeSettings();
        var pipeline = new Mock<IPipeline>();
        var sut = CreateSut();

        var request = sut.NewRequest(settings.RequestConfigurations[0], pipeline.Object, CurrentTime);
        var result = sut.GetPipeline(request);
        result.Should().BeSameAs(pipeline.Object);
        pipeline.Verify(x => x.SetRequest(settings.RequestConfigurations[0], request), Times.Once());
    }
}
