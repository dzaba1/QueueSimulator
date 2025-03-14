using Dzaba.TeamCitySimulator.Lib.Events;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Dzaba.TeamCitySimulator.Lib.Tests;

[TestFixture]
public class EventQueueTests
{
    [Test]
    public void EnqueueDequeue_WhenNewEventAdded_ThenFirstOlderIsTaken()
    {
        var sut = new EventQueue();

        var startTime = new DateTime(2025, 1, 1);
        var list = new List<int>();

        sut.Enqueue("2", startTime.AddHours(2), e => list.Add(2));
        sut.Enqueue("1", startTime.AddHours(1), e => list.Add(1));

        sut.Dequeue().Invoke();
        sut.Dequeue().Invoke();

        list.Should().HaveCount(2);
        list[0].Should().Be(1);
        list[1].Should().Be(2);
    }

    [Test]
    public void EnqueueDequeue_WhenOlderEventAdded_ThenOlderIsTaken()
    {
        var sut = new EventQueue();

        var startTime = new DateTime(2025, 1, 1);
        var list = new List<int>();

        sut.Enqueue("1", startTime.AddHours(1), e => list.Add(1));
        sut.Enqueue("2", startTime.AddHours(2), e => list.Add(2));
        
        sut.Dequeue().Invoke();
        sut.Dequeue().Invoke();

        list.Should().HaveCount(2);
        list[0].Should().Be(1);
        list[1].Should().Be(2);
    }
}
