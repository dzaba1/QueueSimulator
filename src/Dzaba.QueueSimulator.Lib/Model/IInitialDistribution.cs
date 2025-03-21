using Dzaba.QueueSimulator.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dzaba.QueueSimulator.Lib.Model;

[JsonDerivedType(typeof(IntervalInitialDistribution), "interval")]
[JsonDerivedType(typeof(DurationDistribution), "duration")]
public interface IInitialDistribution
{
    IEnumerable<DateTimeOffset> GetInitTimes(DateTime simulationStartTime, IRand rand);
}
