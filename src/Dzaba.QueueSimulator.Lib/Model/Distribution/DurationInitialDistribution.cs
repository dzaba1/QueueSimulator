using Dzaba.QueueSimulator.Lib.Utils;
using System;
using System.Collections.Generic;

namespace Dzaba.QueueSimulator.Lib.Model.Distribution;

public sealed class DurationInitialDistribution : IInitialDistribution
{
    public IDuration Duration { get; set; }
    public ushort NumberToQueue { get; set; }

    public IEnumerable<DateTimeOffset> GetInitTimes(DateTime simulationStartTime, IRand rand)
    {
        var waitTime = Duration.Get(rand) / NumberToQueue;
        for (var i = 0; i < NumberToQueue; i++)
        {
            yield return simulationStartTime + waitTime * i;
        }
    }
}