using System;
using System.ComponentModel.DataAnnotations;

namespace Dzaba.QueueSimulator.Lib.Model;

public sealed class RequestConfiguration
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; }

    public TimeSpan? Duration { get; set; }

    [Required]
    [MinLength(1)]
    public string[] CompatibleAgents { get; set; }

    public string[] RequestDependencies { get; set; }
    public bool IsComposite { get; set; }
}
