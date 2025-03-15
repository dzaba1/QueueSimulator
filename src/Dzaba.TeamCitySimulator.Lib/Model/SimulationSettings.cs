using System;
using System.ComponentModel.DataAnnotations;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class SimulationSettings
{
    public int? MaxRunningAgents { get; set; }
    public TimeSpan SimulationDuration { get; set; } = TimeSpan.FromHours(8);

    [Required]
    [MinLength(1)]
    public AgentConfiguration[] Agents { get; set; }

    [Required]
    [MinLength(1)]
    public BuildConfiguration[] BuildConfigurations { get; set; }

    [Required]
    [MinLength(1)]
    public QueuedBuild[] QueuedBuilds { get; set; }
}
