using System;
using System.ComponentModel.DataAnnotations;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class Build
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; }

    public TimeSpan Duration { get; set; }

    [Required]
    [MinLength(1)]
    public string[] CompatibleAgents { get; set; }

    public string[] BuildDependencies { get; set; }
    public bool IsComposite { get; set; }
}
