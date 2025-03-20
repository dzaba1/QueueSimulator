using AutoFixture;
using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Dzaba.TestUtils;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Tests.Events;

[TestFixture]
public class QueueRequestEventHandlerTests
{
    private static readonly DateTime CurrentTime = DateTime.Now;

    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private QueueRequestEventHandler CreateSut()
    {
        return fixture.Create<QueueRequestEventHandler>();
    }

    [Test]
    public void Handle_WhenRequestConfigProvided_ThenCreateAgentEventIsMade()
    {
        var eventData = new EventData("Test", CurrentTime);
        var payload = new SimulationPayload(new SimulationSettings
        {
            Agents = [],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1"
                }
            ],
        });
        var initRequestConfig = payload.SimulationSettings.RequestConfigurations[0];
        var pipeline = new Pipeline(initRequestConfig, payload);
        var eventPayload = new QueueRequestEventPayload(initRequestConfig, pipeline, null);

        var request = new Request();

        fixture.FreezeMock<IRequestsRepository>()
            .Setup(x => x.NewRequest(initRequestConfig, pipeline, eventData.Time))
            .Returns(request);
        var eventsQueue = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, eventPayload);

        eventsQueue.Verify(x => x.AddCreateAgentQueueEvent(request, eventData.Time), Times.Once());
        request.State.Should().Be(RequestState.WaitingForAgent);
    }

    [Test]
    public void Handle_WhenRequestWithDependency_ThenDependencyIsScheduled()
    {
        var eventData = new EventData("Test", CurrentTime);
        var payload = new SimulationPayload(new SimulationSettings
        {
            Agents = [],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    RequestDependencies = ["BuildConfig2"]
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig2"
                }
            ],
        });
        var initRequestConfig = payload.SimulationSettings.RequestConfigurations[0];
        var pipeline = new Pipeline(initRequestConfig, payload);
        var eventPayload = new QueueRequestEventPayload(initRequestConfig, pipeline, null);

        var request = new Request();

        fixture.FreezeMock<IRequestsRepository>()
            .Setup(x => x.NewRequest(initRequestConfig, pipeline, eventData.Time))
            .Returns(request);
        var eventsQueue = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, eventPayload);

        eventsQueue.Verify(x => x.AddCreateAgentQueueEvent(It.IsAny<Request>(), It.IsAny<DateTime>()), Times.Never());
        request.State.Should().Be(RequestState.WaitingForDependencies);
        eventsQueue.Verify(x => x.AddQueueRequestQueueEvent(It.Is<QueueRequestEventPayload>(p => ValidateQueueRequestPayload(p, payload.SimulationSettings.RequestConfigurations[1], request, pipeline)), eventData.Time), Times.Once());
    }

    private bool ValidateQueueRequestPayload(QueueRequestEventPayload actual,
        RequestConfiguration requestConfiguration, Request parent, IPipeline pipeline)
    {
        return actual.Parent == parent && actual.RequestConfiguration == requestConfiguration && actual.Pipeline == pipeline;
    }

    [Test]
    public void Handle_WhenParent_ThenRequestIsScheduled()
    {
        var eventData = new EventData("Test", CurrentTime);
        var payload = new SimulationPayload(new SimulationSettings
        {
            Agents = [],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    RequestDependencies = ["BuildConfig2"]
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig2"
                }
            ],
        });

        var parentRequest = new Request
        {
            Id = 1,
            RequestConfiguration = "BuildConfig1"
        };

        var initRequestConfig = payload.SimulationSettings.RequestConfigurations[0];
        var currentRequestConfig = payload.SimulationSettings.RequestConfigurations[1];
        var pipeline = new Pipeline(initRequestConfig, payload);
        var eventPayload = new QueueRequestEventPayload(currentRequestConfig, pipeline, parentRequest);

        var request = new Request();

        fixture.FreezeMock<IRequestsRepository>()
            .Setup(x => x.NewRequest(currentRequestConfig, pipeline, eventData.Time))
            .Returns(request);
        var eventsQueue = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, eventPayload);

        eventsQueue.Verify(x => x.AddCreateAgentQueueEvent(request, eventData.Time), Times.Once());
        request.State.Should().Be(RequestState.WaitingForAgent);

        pipeline.GetChildren(parentRequest).First().Should().BeSameAs(request);
        pipeline.GetParents(request).First().Should().BeSameAs(parentRequest);
    }

    [Test]
    public void Handle_WhenAllChildrenFinished_ThenSchedule()
    {
        var eventData = new EventData("Test", CurrentTime);
        var payload = new SimulationPayload(new SimulationSettings
        {
            Agents = [],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    RequestDependencies = ["BuildConfig2", "BuildConfig3", "BuildConfig4"]
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig2"
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig3"
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig4"
                }
            ],
        });
        var initRequestConfig = payload.SimulationSettings.RequestConfigurations[0];
        var pipeline = new Mock<IPipeline>();
        pipeline.Setup(x => x.RequestConfigurationsGraph)
            .Returns(new RequestConfigurationsGraph(payload, initRequestConfig));

        var eventPayload = new QueueRequestEventPayload(initRequestConfig, pipeline.Object, null);

        var request = new Request();
        var childrenRequests = new List<Request>();

        for (int i = 0; i < 3; i++)
        {
            var childRequest = new Request
            {
                State = RequestState.Finished,
                Id = i
            };
            childrenRequests.Add(childRequest);
            pipeline.Setup(x => x.TryGetRequest(payload.SimulationSettings.RequestConfigurations[i+1], out childRequest))
                .Returns(true);
        }

        fixture.FreezeMock<IRequestsRepository>()
            .Setup(x => x.NewRequest(initRequestConfig, pipeline.Object, eventData.Time))
            .Returns(request);
        var eventsQueue = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, eventPayload);

        eventsQueue.Verify(x => x.AddCreateAgentQueueEvent(request, eventData.Time), Times.Once());
        request.State.Should().Be(RequestState.WaitingForAgent);
        eventsQueue.Verify(x => x.AddQueueRequestQueueEvent(It.IsAny<QueueRequestEventPayload>(), It.IsAny<DateTime>()), Times.Never());

        for (int i = 0; i < 3; i++)
        {
            pipeline.Verify(x => x.SetReference(childrenRequests[i], request), Times.Once());
        }
    }

    [Test]
    public void Handle_WhenAllChildrenRunning_ThenDontSchedule()
    {
        var eventData = new EventData("Test", CurrentTime);
        var payload = new SimulationPayload(new SimulationSettings
        {
            Agents = [],
            RequestConfigurations = [
                new RequestConfiguration
                {
                    Name = "BuildConfig1",
                    RequestDependencies = ["BuildConfig2", "BuildConfig3", "BuildConfig4"]
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig2"
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig3"
                },
                new RequestConfiguration
                {
                    Name = "BuildConfig4"
                }
            ],
        });
        var initRequestConfig = payload.SimulationSettings.RequestConfigurations[0];
        var pipeline = new Mock<IPipeline>();
        pipeline.Setup(x => x.RequestConfigurationsGraph)
            .Returns(new RequestConfigurationsGraph(payload, initRequestConfig));

        var eventPayload = new QueueRequestEventPayload(initRequestConfig, pipeline.Object, null);

        var request = new Request();
        var childrenRequests = new List<Request>();

        for (int i = 0; i < 3; i++)
        {
            var childRequest = new Request
            {
                State = RequestState.Finished,
                Id = i
            };

            if (i == 2)
            {
                childRequest.State = RequestState.Running;
            }

            childrenRequests.Add(childRequest);
            pipeline.Setup(x => x.TryGetRequest(payload.SimulationSettings.RequestConfigurations[i + 1], out childRequest))
                .Returns(true);
        }

        fixture.FreezeMock<IRequestsRepository>()
            .Setup(x => x.NewRequest(initRequestConfig, pipeline.Object, eventData.Time))
            .Returns(request);
        var eventsQueue = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, eventPayload);

        eventsQueue.Verify(x => x.AddCreateAgentQueueEvent(It.IsAny<Request>(), It.IsAny<DateTime>()), Times.Never());
        request.State.Should().Be(RequestState.WaitingForDependencies);
        eventsQueue.Verify(x => x.AddQueueRequestQueueEvent(It.IsAny<QueueRequestEventPayload>(), It.IsAny<DateTime>()), Times.Never());

        for (int i = 0; i < 3; i++)
        {
            pipeline.Verify(x => x.SetReference(childrenRequests[i], request), Times.Once());
        }
    }
}
