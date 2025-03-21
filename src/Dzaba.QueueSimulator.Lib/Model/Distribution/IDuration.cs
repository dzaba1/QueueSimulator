using Dzaba.QueueSimulator.Lib.Utils;
using System;
using System.Text.Json.Serialization;

namespace Dzaba.QueueSimulator.Lib.Model.Distribution;

[JsonDerivedType(typeof(StaticDuration), "static")]
[JsonDerivedType(typeof(MinMaxDuration), "minMax")]
public interface IDuration
{
    TimeSpan Get(IRand random);
}
