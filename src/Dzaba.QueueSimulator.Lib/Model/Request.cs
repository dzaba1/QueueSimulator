using System;
using System.Diagnostics;

namespace Dzaba.QueueSimulator.Lib.Model;

[DebuggerDisplay("{Id} [{RequestConfiguration}]")]
public sealed class Request
{
    public long Id { get; set; }
    public string RequestConfiguration { get; set; }
    public RequestState State { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? StartTime { get; set; }
    public long? AgentId { get; set; }
    public DateTime? EndTime { get; set; }
    public long[] Dependencies { get; set; }

    public Request ShallowCopy()
    {
        return (Request)MemberwiseClone();
    }
}

public enum RequestState
{
    Created,
    WaitingForDependencies,
    Scheduled,
    WaitingForAgent,
    Running,
    Finished
}