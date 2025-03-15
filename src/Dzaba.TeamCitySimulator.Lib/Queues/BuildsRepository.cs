using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib.Queues;

internal sealed class BuildsRepository
{
    private readonly SimulationPayload simulationPayload;
    private readonly LongSequence buildIdSequence = new();
    private readonly Dictionary<long, Build> allBuilds = new();

    public BuildsRepository(SimulationPayload simulationPayload)
    {
        ArgumentNullException.ThrowIfNull(simulationPayload, nameof(simulationPayload));

        this.simulationPayload = simulationPayload;
    }

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
        return EnumerateBuilds()
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
        return EnumerateBuilds().Count(IsQueued);
    }

    public int GetRunningBuildsCount()
    {
        return EnumerateBuilds().Count(b => b.State == BuildState.Running);
    }

    public IReadOnlyDictionary<string, Build[]> GroupRunningBuildsByBuildConfiguration()
    {
        return EnumerateBuilds()
            .Where(b => b.State == BuildState.Running)
            .GroupBy(b => b.BuildConfiguration)
            .ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<Build> GetWaitingForAgents()
    {
        return EnumerateBuilds()
            .Where(b => b.State == BuildState.WaitingForAgent)
            .Where(b => b.AgentId == null);
    }

    public IEnumerable<Build> GetWaitingForDependencies()
    {
        return EnumerateBuilds()
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

    public IEnumerable<BuildConfiguration> ResolveBuildConfigurationDependencies(BuildConfiguration buildConfiguration, bool recursive)
    {
        ArgumentNullException.ThrowIfNull(buildConfiguration, nameof(buildConfiguration));

        return ResolveBuildConfigurationDependenciesInternal(buildConfiguration, recursive)
            .Distinct(BuildConfigurationNameEqualityComparer.Instance);
    }

    private IEnumerable<BuildConfiguration> ResolveBuildConfigurationDependenciesInternal(BuildConfiguration buildConfiguration, bool recursive)
    {
        if (buildConfiguration.BuildDependencies == null)
        {
            yield break;
        }

        var current = buildConfiguration.BuildDependencies
                .Select(simulationPayload.GetBuildConfiguration);

        foreach (var dep in current)
        {
            yield return dep;

            if (recursive)
            {
                var subDeps = ResolveBuildConfigurationDependencies(dep, true);
                foreach (var subDep in subDeps)
                {
                    yield return subDep;
                }
            }
        }
    }
}
