using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Dzaba.QueueSimulator.Lib.Model;

[DebuggerDisplay("{Name}")]
public sealed class AgentConfiguration
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; }

    public ushort? MaxInstances { get; set; }
    public IDuration InitTime { get; set; }
}
