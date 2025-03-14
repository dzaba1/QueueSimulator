using System;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class SimulationSettings
{
    public ushort MaxQueue { get; set; }
    public TimeSpan? SimulationDuration { get; set; }
}
