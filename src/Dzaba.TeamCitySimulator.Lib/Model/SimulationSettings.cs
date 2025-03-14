using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class SimulationSettings
{
    public ushort? MaxQueue { get; set; }
    public TimeSpan SimulationDuration { get; set; } = TimeSpan.FromHours(8);

    [Required]
    [MinLength(1)]
    public Agent[] Agents { get; set; }

    [Required]
    [MinLength(1)]
    public Build[] BuildConfigurations { get; set; }

    [Required]
    [MinLength(1)]
    public QueuedBuild[] QueuedBuilds { get; set; }

    public IReadOnlyDictionary<string, Build> CacheBuildConfiguration()
    {
        return BuildConfigurations.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, Agent> CacheAgents()
    {
        return Agents.ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
    }
}
