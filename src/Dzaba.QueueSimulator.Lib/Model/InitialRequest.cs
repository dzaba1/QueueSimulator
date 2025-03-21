using Dzaba.QueueSimulator.Lib.Model.Distribution;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Dzaba.QueueSimulator.Lib.Model;

[DebuggerDisplay("{Name}")]
public sealed class InitialRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; }

    [Required]
    public IInitialDistribution Distribution { get; set; }
}
