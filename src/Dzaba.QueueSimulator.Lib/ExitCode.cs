namespace Dzaba.QueueSimulator.Lib;

public enum ExitCode
{
    Ok = 0,
    Unknown = 1,
    AgentNotFound,
    RequestNotFound,
    RequestCyclicDependency,
    CompositeWithDuration,
    CompositeWithAgents,
    CompositeWithoutDependencies
}
