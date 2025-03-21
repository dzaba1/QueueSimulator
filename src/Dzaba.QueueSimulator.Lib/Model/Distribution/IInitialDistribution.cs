using Dzaba.QueueSimulator.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dzaba.QueueSimulator.Lib.Model.Distribution;

[JsonDerivedType(typeof(IntervalInitialDistribution), "interval")]
[JsonDerivedType(typeof(DurationInitialDistribution), "duration")]
public interface IInitialDistribution
{
    IEnumerable<DateTimeOffset> GetInitTimes(DateTime simulationStartTime, IRand rand);
}
