﻿namespace Dzaba.TeamCitySimulator.Lib;

public enum ExitCode
{
    Ok = 0,
    Unknown = 1,
    AgentNotFound,
    RequestNotFound,
    RequestCyclicDependency
}
