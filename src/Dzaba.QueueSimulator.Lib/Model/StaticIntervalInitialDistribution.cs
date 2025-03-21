using Dzaba.QueueSimulator.Lib.Utils;
using System;
using System.Collections.Generic;

namespace Dzaba.QueueSimulator.Lib.Model;

public sealed class IntervalInitialDistribution : IInitialDistribution
{
    public IDuration Interval { get; set; }
    public ushort NumberToQueue { get; set; }

    public IEnumerable<DateTimeOffset> GetInitTimes(DateTime simulationStartTime, IRand rand)
    {
        for (var i = 0; i < NumberToQueue; i++)
        {
            yield return simulationStartTime + Interval.Get(rand) * i;
        }
    }
}
