using AutoFixture;
using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.TestUtils;
using Moq;
using System;

namespace Dzaba.QueueSimulator.Lib.Tests;

internal static class TestUtils
{
    public static Mock<ISimulationContext> SetupSimulationContext(this IFixture fixture, SimulationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(fixture, nameof(fixture));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        var context = fixture.FreezeMock<ISimulationContext>();
        context.Setup(x => x.Payload).Returns(new SimulationPayload(settings));
        return context;
    }
}
