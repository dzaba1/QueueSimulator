namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class Agent
{
    public long Id { get; set; }
    public string AgentConfiguration { get; set; }
    public AgentState State { get; set; }
}

public enum AgentState
{
    Initiated
}