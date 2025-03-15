using System;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class Build
{
    public long Id { get; set; }
    public string BuildConfiguration { get; set; }
    public BuildState State { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? StartTime { get; set; }
    public long? AgentId { get; set; }
    public DateTime? EndTime { get; set; }
    public long[] Dependencies { get; set; }

    public Build ShallowCopy()
    {
        return (Build)MemberwiseClone();
    }
}

public enum BuildState
{
    Created,
    WaitingForDependencies,
    WaitingForAgent,
    Running,
    Finished
}