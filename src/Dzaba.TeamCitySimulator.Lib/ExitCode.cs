namespace Dzaba.TeamCitySimulator.Lib;

public enum ExitCode
{
    Ok = 0,
    Unknown = 1,
    BuildAgentNotFound,
    BuildNotFound,
    BuildCyclicDependency
}
