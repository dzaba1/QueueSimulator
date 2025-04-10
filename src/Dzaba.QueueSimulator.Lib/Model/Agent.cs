﻿using System;
using System.Diagnostics;

namespace Dzaba.QueueSimulator.Lib.Model;

[DebuggerDisplay("{Id} [{AgentConfiguration}]")]
public sealed class Agent
{
    public long Id { get; set; }
    public string AgentConfiguration { get; set; }
    public AgentState State { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }

    public Agent ShallowCopy()
    {
        return (Agent)MemberwiseClone();
    }
}

public enum AgentState
{
    Created,
    Initiating,
    Initiated,
    Running,
    Finished
}