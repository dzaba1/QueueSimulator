using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib.Queues;

internal sealed class BuildsRepository
{
    private readonly LongSequence buildIdSequence = new();
    private readonly Dictionary<long, Build> allBuilds = new();

    public Build NewBuild(BuildConfiguration buildConfiguration, DateTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(buildConfiguration, nameof(buildConfiguration));

        var build = new Build
        {
            Id = buildIdSequence.Next(),
            BuildConfiguration = buildConfiguration.Name,
            CreatedTime = currentTime,
        };
        allBuilds.Add(build.Id, build);

        return build;
    }

    public IReadOnlyDictionary<string, Build[]> GroupQueueByBuildConfiguration()
    {
        return allBuilds.Values
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
        return allBuilds.Values.Count(IsQueued);
    }

    public int GetRunningBuildsCount()
    {
        return allBuilds.Values.Count(b => b.State == BuildState.Running);
    }

    public IReadOnlyDictionary<string, Build[]> GroupRunningBuildsByBuildConfiguration()
    {
        return allBuilds.Values
            .Where(b => b.State == BuildState.Running)
            .GroupBy(b => b.BuildConfiguration)
            .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<Build> GetWaitingForAgents()
    {
        return allBuilds.Values
            .Where(b => b.State == BuildState.WaitingForAgent)
            .Where(b => b.AgentId == null);
    }

    public IEnumerable<Build> GetWaitingForDependencies()
    {
        return allBuilds.Values
            .Where(b => b.State == BuildState.WaitingForDependencies);
    }

    public IEnumerable<Build> EnumerateBuilds()
    {
        return allBuilds.Values;
    }

    public Build GetBuild(long id)
    {
        return allBuilds[id];
    }
}
