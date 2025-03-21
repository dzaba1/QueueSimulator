using Dzaba.QueueSimulator.Lib.Utils;
using System;

namespace Dzaba.QueueSimulator.Lib.Model;

public sealed class StaticDuration : IDuration
{
    public TimeSpan Value { get; set; }

    public TimeSpan Get(IRand random)
    {
        return Value;
    }
}
