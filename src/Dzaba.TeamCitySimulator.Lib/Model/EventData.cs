using System;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class EventData
{
    public DateTime Timestamp { get; set; }
    public string Name { get; set; }
    public ushort QueueLength { get; set; }
    public AgentQueueData[] AgentQueues { get; set; }
}

public sealed class AgentQueueData
{
    public string Name { get; set; }
    public ushort Length { get; set; }
}
