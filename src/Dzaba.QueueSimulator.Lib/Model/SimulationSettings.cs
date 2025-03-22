using System.ComponentModel.DataAnnotations;

namespace Dzaba.QueueSimulator.Lib.Model;

public sealed class SimulationSettings
{
    public int? MaxRunningAgents { get; set; }
    public bool IncludeAllRequests { get; set; }
    public bool IncludeAllAgents { get; set; }

    [Required]
    public AgentConfiguration[] Agents { get; set; }

    [Required]
    public RequestConfiguration[] RequestConfigurations { get; set; }

    [Required]
    public InitialRequest[] InitialRequests { get; set; }

    public string[] RequestConfigurationsToObserve { get; set; }
    public string[] AgentConfigurationsToObserve { get; set; }
}
