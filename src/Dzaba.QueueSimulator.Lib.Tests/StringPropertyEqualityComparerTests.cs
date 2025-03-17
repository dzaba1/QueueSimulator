using Dzaba.QueueSimulator.Lib.Model;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace Dzaba.QueueSimulator.Lib.Tests;

[TestFixture]
public class StringPropertyEqualityComparerTests
{
    private StringPropertyEqualityComparer<TestClass> CreateSut(StringComparer comparer)
    {
        return new StringPropertyEqualityComparer<TestClass>(x => x.Value, comparer);
    }

    [Test]
    public void Equals_WhenTwoDifferentClassAndIgnoreCase_ThenObjectsMatch()
    {
        var obj1 = new TestClass
        {
            Value = "test"
        };
        var obj2 = new TestClass
        {
            Value = "TEST"
        };

        var sut = CreateSut(StringComparer.OrdinalIgnoreCase);

        sut.Equals(obj1, obj2).Should().BeTrue();
        sut.GetHashCode(obj1).Should().Be(sut.GetHashCode(obj2));
    }

    private class TestClass
    {
        public string Value { get; set; }
    }
}
