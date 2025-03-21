using Dzaba.QueueSimulator.Lib.Utils;
using System;

namespace Dzaba.QueueSimulator.Lib.Model.Distribution;

public sealed class MinMaxDuration : IDuration
{
    public TimeSpan Min { get; set; }
    public TimeSpan Max { get; set; }

    public TimeSpan Get(IRand random)
    {
        ArgumentNullException.ThrowIfNull(random, nameof(random));

        var ticks = random.NextLong(Min.Ticks, Max.Ticks);
        return TimeSpan.FromTicks(ticks);
    }
}