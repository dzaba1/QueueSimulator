using System;

namespace Dzaba.QueueSimulator.Lib.Model;

public sealed class SimulationReport
{
    public TimeEventData[] Events { get; set; }
    public RequestDurationStatistics RequestDurationStatistics { get; set; }
}

public sealed class RequestDurationStatistics
{
    public ElementsData<TimeSpan> AvgRequestDuration { get; set; }
    public ElementsData<TimeSpan> MinRequestDuration { get; set; }
    public ElementsData<TimeSpan> MaxRequestDuration { get; set; }
}