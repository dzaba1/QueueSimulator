using System;
using System.Diagnostics;

namespace Dzaba.QueueSimulator.Lib.Model;

[DebuggerDisplay("{Id} [{RequestConfiguration}]")]
public sealed class Request
{
    public long Id { get; set; }
    public string RequestConfiguration { get; set; }
    public RequestState State { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public long? AgentId { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public long[] Dependencies { get; set; }

    public Request ShallowCopy()
    {
        return (Request)MemberwiseClone();
    }

    public TimeSpan? RunningDuration()
    {
        if (State != RequestState.Finished)
        {
            return null;
        }

        return EndTime.Value - StartTime.Value;
    }

    public TimeSpan? QueueDuration()
    {
        if (State < RequestState.Running)
        {
            return null;
        }

        return CreatedTime - StartTime.Value;
    }

    public TimeSpan? TotalDuration()
    {
        if (State != RequestState.Finished)
        {
            return null;
        }

        return CreatedTime - StartTime.Value;
    }
}

public enum RequestState
{
    Created,
    WaitingForDependencies,
    WaitingForAgent,
    WaitingForAgentStart,
    Running,
    Finished
}