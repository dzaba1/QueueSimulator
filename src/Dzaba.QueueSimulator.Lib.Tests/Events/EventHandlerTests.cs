using AutoFixture;
using Dzaba.QueueSimulator.Lib.Events;
using Dzaba.TestUtils;
using Moq;
using NUnit.Framework;
using System;

namespace Dzaba.QueueSimulator.Lib.Tests.Events;

[TestFixture]
public class EventHandlerTests
{
    private static readonly DateTime CurrentTime = DateTime.Now;

    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private TestEventHandler CreateSut()
    {
        return fixture.Create<TestEventHandler>();
    }

    [Test]
    public void Handle_WhenHandled_ThenTimedEventIsSaved()
    {
        var payload = new TestPayload(new EventData("TestEvent", CurrentTime));

        var events = fixture.FreezeMock<ISimulationEvents>();

        var sut = CreateSut();

        sut.Handle(payload);

        events.Verify(x => x.AddTimedEventData(payload.EventData, "Test"), Times.Once());
    }

    private class TestPayload : EventDataPayload
    {
        public TestPayload(EventData eventData) : base(eventData)
        {
        }
    }

    private class TestEventHandler : Lib.Events.EventHandler<TestPayload>
    {
        public TestEventHandler(ISimulationEvents simulationEvents) : base(simulationEvents)
        {
        }

        protected override string OnHandle(TestPayload payload)
        {
            return "Test";
        }
    }
}
