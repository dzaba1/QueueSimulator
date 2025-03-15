using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib.Queues;

internal sealed class BuildQueue
{
    private readonly LongSequence buildIdSequence = new();
    private readonly List<Build> allBuilds = new();

    public Build NewBuild(BuildConfiguration buildConfiguration, DateTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(buildConfiguration, nameof(buildConfiguration));

        var build = new Build
        {
            Id = buildIdSequence.Next(),
            BuildConfiguration = buildConfiguration.Name,
            CreatedTime = currentTime,
        };
        allBuilds.Add(build);

        return build;
    }

    public IReadOnlyDictionary<string, Build[]> GroupQueueByBuildConfiguration()
    {
        return allBuilds
            .Where(IsQueued)
            .GroupBy(b => b.BuildConfiguration)
            .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    private bool IsQueued(Build build)
    {
        return build.State != BuildState.Running && build.State != BuildState.Finished;
    }

    public int GetQueueLength()
    {
        return allBuilds.Count(IsQueued);
    }

    public int GetRunningBuildsCount()
    {
        return allBuilds.Count(b => b.State == BuildState.Running);
    }

    public IReadOnlyDictionary<string, Build[]> GroupRunningBuildsByBuildConfiguration()
    {
        return allBuilds
            .Where(b => b.State == BuildState.Running)
            .GroupBy(b => b.BuildConfiguration)
            .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<Build> GetWaitingForAgents()
    {
        return allBuilds
            .Where(b => b.State == BuildState.WaitingForAgent)
            .Where(b => b.AgentId == null);
    }

    public IEnumerable<Build> EnumerateBuilds()
    {
        return allBuilds;
    }
}
