using System;
using System.ComponentModel.DataAnnotations;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class AgentConfiguration
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; }

    public ushort? MaxInstances { get; set; }
    public TimeSpan? InitTime { get; set; }
}
