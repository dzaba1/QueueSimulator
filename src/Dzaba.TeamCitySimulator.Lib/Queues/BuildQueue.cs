using Dzaba.TeamCitySimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib.Queues;

internal sealed class BuildQueue
{
    private readonly LongSequence buildIdSequence = new();
    private readonly List<Build> builds = new();

    public Build NewBuild(BuildConfiguration buildConfiguration, DateTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(buildConfiguration, nameof(buildConfiguration));

        var build = new Build
        {
            Id = buildIdSequence.Next(),
            BuildConfiguration = buildConfiguration.Name,
            CreatedTime = currentTime,
        };
        builds.Add(build);

        return build;
    }

    public int Count => builds.Count;

    public IReadOnlyDictionary<string, Build[]> GroupByBuildConfiguration()
    {
        return builds
            .GroupBy(b => b.BuildConfiguration)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
