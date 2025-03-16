using System.ComponentModel.DataAnnotations;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class InitialRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; }

    public ushort NumberToQueue { get; set; }
}
