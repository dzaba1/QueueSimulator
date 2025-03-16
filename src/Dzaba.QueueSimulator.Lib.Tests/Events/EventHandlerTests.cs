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
        var eventData = new EventData("TestEvent", CurrentTime);

        var events = fixture.FreezeMock<ISimulationEvents>();

        var sut = CreateSut();

        sut.Handle(eventData, "Data");

        events.Verify(x => x.AddTimedEventData(eventData, "Data"), Times.Once());
    }

    private class TestEventHandler : Lib.Events.EventHandler<string>
    {
        public TestEventHandler(ISimulationEvents simulationEvents) : base(simulationEvents)
        {
        }

        protected override string OnHandle(EventData eventData, string payload)
        {
            return payload;
        }
    }
}
