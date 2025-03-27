using System.ComponentModel.DataAnnotations;

namespace Dzaba.QueueSimulator.Lib.Model;

public sealed class SimulationSettings
{
    public int? MaxRunningAgents { get; set; }
    
    [Required]
    public AgentConfiguration[] Agents { get; set; }

    [Required]
    public RequestConfiguration[] RequestConfigurations { get; set; }

    [Required]
    public InitialRequest[] InitialRequests { get; set; }

    public ReportSettings ReportSettings { get; set; } = new ReportSettings();
}
