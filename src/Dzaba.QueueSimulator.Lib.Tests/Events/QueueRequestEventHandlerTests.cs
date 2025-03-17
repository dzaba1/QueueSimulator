using AutoFixture;
using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib.Repositories;
using Dzaba.TestUtils;
using NUnit.Framework;
using System;

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
        var eventPayload = new QueueRequestEventPayload(initRequestConfig, new Pipeline(initRequestConfig, payload), null);

        var request = new Request();

        fixture.FreezeMock<IRequestsRepository>()
            .Setup(x => x.NewRequest(initRequestConfig, eventData.Time))
            .Returns(request);
        var eventsQueue = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, eventPayload);

        eventsQueue.Verify(x => x.AddCreateAgentQueueEvent(request, eventData.Time));
    }
}
