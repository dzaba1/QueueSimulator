using System;

namespace Dzaba.QueueSimulator.Lib.Model;

public sealed class RequestConfigurationStatistics
{
    public string Name { get; set; }
    public TimeSpan AvgDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public TimeSpan AvgQueueTime { get; set; }
    public TimeSpan MinQueueTime { get; set; }
    public TimeSpan MaxQueueTime { get; set; }
    public TimeSpan AvgTotalDuration { get; set; }
    public TimeSpan MinTotalDuration { get; set; }
    public TimeSpan MaxTotalDuration { get; set; }
}