using System;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class Agent
{
    public long Id { get; set; }
    public string AgentConfiguration { get; set; }
    public AgentState State { get; set; }
    public DateTime CreatedTime { get; set; }
}

public enum AgentState
{
    Created,
    Initiating,
    Running,
    Finished
}