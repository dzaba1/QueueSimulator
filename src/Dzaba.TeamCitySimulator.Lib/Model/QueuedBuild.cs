using System.ComponentModel.DataAnnotations;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class QueuedBuild
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; }

    public ushort BuildsToQueue { get; set; }
}
