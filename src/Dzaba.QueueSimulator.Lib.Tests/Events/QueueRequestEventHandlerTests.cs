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
        var payload = new QueueRequestEventPayload(new EventData("Test", CurrentTime), new RequestConfiguration());

        var request = new Request();

        fixture.FreezeMock<IRequestsRepository>()
            .Setup(x => x.NewRequest(payload.RequestConfiguration, payload.EventData.Time))
            .Returns(request);
        var eventsQueue = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(payload);

        eventsQueue.Verify(x => x.AddCreateAgentQueueEvent(request, payload.EventData.Time));
    }
}
