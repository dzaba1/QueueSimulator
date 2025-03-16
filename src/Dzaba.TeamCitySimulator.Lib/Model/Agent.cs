using System;

namespace Dzaba.QueueSimulator.Lib.Model;

public sealed class Agent
{
    public long Id { get; set; }
    public string AgentConfiguration { get; set; }
    public AgentState State { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? EndTime { get; set; }

    public Agent ShallowCopy()
    {
        return (Agent)MemberwiseClone();
    }
}

public enum AgentState
{
    Created,
    Initiating,
    Running,
    Finished
}