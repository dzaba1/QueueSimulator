using CsvHelper;
using Dzaba.QueueSimulator.Lib.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Dzaba.QueueSimulator.Lib;

public interface ICsvSerializer
{
    string Serialize(IEnumerable<TimeEventData> timedEvents, SimulationSettings simulationSettings);
}

internal sealed class CsvSerializer : ICsvSerializer
{
    public string Serialize(IEnumerable<TimeEventData> timedEvents, SimulationSettings simulationSettings)
    {
        ArgumentNullException.ThrowIfNull(timedEvents, nameof(timedEvents));
        ArgumentNullException.ThrowIfNull(simulationSettings, nameof(simulationSettings));

        var filtered = timedEvents
            .GroupBy(e => e.Timestamp)
            .OrderBy(g => g.Key)
            .Select(g => g.Last());

        using var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        var headers = GetHeaders(simulationSettings);

        foreach (var heading in headers)
        {
            csv.WriteField(heading);
        }

        csv.NextRecord();

        foreach (var item in filtered)
        {
            var values = GetValues(item, simulationSettings);
            foreach (var value in values)
            {
                csv.WriteField(value);
            }

            csv.NextRecord();
        }

        stream.Flush();
        stream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private IEnumerable<string> GetHeaders(SimulationSettings simulationSettings)
    {
        yield return "Timestamp";
        yield return "Name";
        yield return "Message";

        yield return "TotalRunningAgents";
        yield return "TotalRunningRequests";
        yield return "TotalRequestsQueue";

        foreach (var agent in simulationSettings.Agents)
        {
            yield return $"RunningAgent_{agent.Name}";
        }

        foreach (var request in simulationSettings.RequestConfigurations)
        {
            yield return $"RunningRequests_{request.Name}";
            yield return $"RequestsQueue_{request.Name}";
        }
    }

    private IEnumerable<object> GetValues(TimeEventData timeEvent, SimulationSettings simulationSettings)
    {
        yield return timeEvent.Timestamp;
        yield return timeEvent.Name;
        yield return timeEvent.Message;

        yield return timeEvent.RunningAgents.Total;
        yield return timeEvent.RunningRequests.Total;
        yield return timeEvent.RequestsQueue.Total;

        var runningAgentDict = timeEvent.RunningAgents.ToDictionary();
        foreach (var agent in simulationSettings.Agents)
        {
            if (runningAgentDict.TryGetValue(agent.Name, out var value))
            {
                yield return value;
            }
            else
            {
                yield return 0;
            }
        }

        var runningRequestsDict = timeEvent.RunningRequests.ToDictionary();
        var requestsQueueDict = timeEvent.RequestsQueue.ToDictionary();
        foreach (var request in simulationSettings.RequestConfigurations)
        {
            if (runningRequestsDict.TryGetValue(request.Name, out var value))
            {
                yield return value;
            }
            else
            {
                yield return 0;
            }

            if (requestsQueueDict.TryGetValue(request.Name, out value))
            {
                yield return value;
            }
            else
            {
                yield return 0;
            }
        }
    }
}
