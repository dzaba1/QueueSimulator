using Serilog.Events;

namespace Dzaba.QueueSimulator.Cmd;

public sealed class CustomLoggingOptions
{
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Debug;
    public LogEventLevel? ConsoleLevel { get; set; }
    public LogEventLevel? FileLevel { get; set; }
}
