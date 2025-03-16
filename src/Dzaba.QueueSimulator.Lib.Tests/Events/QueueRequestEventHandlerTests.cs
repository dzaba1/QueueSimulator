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
        var requestConfiguration = new RequestConfiguration();

        var request = new Request();

        fixture.FreezeMock<IRequestsRepository>()
            .Setup(x => x.NewRequest(requestConfiguration, eventData.Time))
            .Returns(request);
        var eventsQueue = fixture.FreezeMock<ISimulationEventQueue>();

        var sut = CreateSut();

        sut.Handle(eventData, requestConfiguration);

        eventsQueue.Verify(x => x.AddCreateAgentQueueEvent(request, eventData.Time));
    }
}
