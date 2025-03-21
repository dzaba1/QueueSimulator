using Dzaba.QueueSimulator.Lib.Utils;
using System;
using System.Text.Json.Serialization;

namespace Dzaba.QueueSimulator.Lib.Model;

[JsonDerivedType(typeof(StaticDuration), "static")]
public interface IDuration
{
    TimeSpan Get(IRand random);
}
