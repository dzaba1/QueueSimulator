using Dzaba.QueueSimulator.Lib.Model;
using FluentAssertions;
using NUnit.Framework;

namespace Dzaba.QueueSimulator.Lib.Tests;

[TestFixture]
public class OnePropertyComparerTests
{
    private OnePropertyComparer<TestClass<T>, T> CreateSut<T>()
    {
        return new OnePropertyComparer<TestClass<T>, T>(x => x.Value);
    }

    private void TestTypeWithDefaultComparer<T>(T value1, T value2)
    {
        var obj1 = new TestClass<T>
        {
            Value = value1
        };
        var obj2 = new TestClass<T>
        {
            Value = value2
        };

        var sut = CreateSut<T>();

        sut.Equals(obj1, obj2).Should().BeTrue();
        sut.GetHashCode(obj1).Should().Be(sut.GetHashCode(obj2));
    }

    [Test]
    public void Equals_WhenLong_ThenDefaultComparerWorks()
    {
        TestTypeWithDefaultComparer<long>(1, 1);
    }

    private class TestClass<T>
    {
        public T Value { get; set; }
    }
}
