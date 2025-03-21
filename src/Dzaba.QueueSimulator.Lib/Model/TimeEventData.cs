using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib.Model;

[DebuggerDisplay("{Timestamp} [{Name}] - {Message}")]
public sealed class TimeEventData
{
    public DateTime Timestamp { get; set; }
    public string Name { get; set; }
    public string Message { get; set; }
    public ElementsData RequestsQueue { get; set; }
    public ElementsData RunningAgents { get; set; }
    public ElementsData RunningRequests { get; set; }
    public Agent[] AllAgents { get; set; }
    public Request[] AllRequests { get; set; }
}

[DebuggerDisplay("{Name}: {Length}")]
public sealed class NamedQueueData
{
    public string Name { get; set; }
    public int Length { get; set; }
}

public sealed class ElementsData
{
    public int Total { get; set; }
    public NamedQueueData[] Grouped { get; set; }

    public IReadOnlyDictionary<string, int> ToDictionary()
    {
        return Grouped.ToDictionary(g => g.Name, g => g.Length, StringComparer.OrdinalIgnoreCase);
    }
}
