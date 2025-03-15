﻿using System;

namespace Dzaba.TeamCitySimulator.Lib.Model;

public sealed class TimeEventData
{
    public DateTime Timestamp { get; set; }
    public string Name { get; set; }
    public string Message { get; set; }
    public ElementsData BuildsQueue { get; set; }
    public ElementsData RunningAgents { get; set; }
    public ElementsData RunningBuilds { get; set; }
    public Agent[] AllAgents { get; set; }
    public Build[] AllBuilds { get; set; }
}

public sealed class NamedQueueData
{
    public string Name { get; set; }
    public int Length { get; set; }
}

public sealed class ElementsData
{
    public int Total { get; set; }
    public NamedQueueData[] Grouped { get; set; }
}
