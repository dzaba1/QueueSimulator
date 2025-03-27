namespace Dzaba.QueueSimulator.Lib.Model;

public sealed class ReportSettings
{
    public bool IncludeAllRequests { get; set; }
    public bool IncludeAllAgents { get; set; }
    public string[] RequestConfigurationsToObserve { get; set; }
    public string[] AgentConfigurationsToObserve { get; set; }
    public bool CsvSaveTimestampTicks { get; set; }
}