using System;
using System.Diagnostics;

namespace Dzaba.QueueSimulator.Lib.Model;

[DebuggerDisplay("{Timestamp} [{Name}] - {Message}")]
public sealed class TimeEventData
{
    public DateTime Timestamp { get; set; }
    public string Name { get; set; }
    public string Message { get; set; }
    public ElementsData<int> RequestsQueue { get; set; }
    public ElementsData<int> RunningAgents { get; set; }
    public ElementsData<int> RunningRequests { get; set; }
    public Agent[] AllAgents { get; set; }
    public Request[] AllRequests { get; set; }
}
