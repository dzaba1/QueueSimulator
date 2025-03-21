namespace Dzaba.QueueSimulator.Lib.Model;

public sealed class SimulationReport
{
    public TimeEventData[] Events { get; set; }
    public RequestConfigurationStatistics[] RequestDurationStatistics { get; set; }
}
