using Dzaba.QueueSimulator.Lib.Model.Distribution;
using Dzaba.QueueSimulator.Lib.Utils;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;

namespace Dzaba.QueueSimulator.Lib.Tests;

[TestFixture]
public class MinMaxDurationTests
{
    [Test]
    public void AA()
    {
        var sut = new MinMaxDuration
        {
            Min = TimeSpan.FromHours(1),
            Max = TimeSpan.FromHours(3),
        };

        var rand = new Mock<IRand>();
        rand.Setup(x => x.NextLong(It.IsAny<long>(), It.IsAny<long>()))
            .Returns<long, long>((min, max) =>
            {
                return min + ((max - min) / 2);
            });

        var result = sut.Get(rand.Object);
        result.Should().Be(TimeSpan.FromHours(2));
    }
}
