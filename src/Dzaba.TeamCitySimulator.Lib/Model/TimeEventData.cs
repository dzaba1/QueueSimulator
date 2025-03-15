using System;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class TimeEventData
{
    public DateTime Timestamp { get; set; }
    public string Name { get; set; }
    public int QueueLength { get; set; }
    public uint TotalRunningBuilds { get; set; }
    public NamedQueueData[] AgentQueues { get; set; }
    public NamedQueueData[] BuildConfigurationQueues { get; set; }
}

public sealed class NamedQueueData
{
    public string Name { get; set; }
    public int Length { get; set; }
}
