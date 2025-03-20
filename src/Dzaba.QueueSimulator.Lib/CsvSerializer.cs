using CsvHelper;
using CsvHelper.Configuration;
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
    void Serialize(Stream stream, IEnumerable<TimeEventData> timedEvents, SimulationSettings simulationSettings);
}

internal sealed class CsvSerializer : ICsvSerializer
{
    public string Serialize(IEnumerable<TimeEventData> timedEvents, SimulationSettings simulationSettings)
    {
        ArgumentNullException.ThrowIfNull(timedEvents, nameof(timedEvents));
        ArgumentNullException.ThrowIfNull(simulationSettings, nameof(simulationSettings));

        using var stream = new MemoryStream();
        
        Serialize(stream, timedEvents, simulationSettings);

        stream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void Serialize(Stream stream, IEnumerable<TimeEventData> timedEvents, SimulationSettings simulationSettings)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        ArgumentNullException.ThrowIfNull(timedEvents, nameof(timedEvents));
        ArgumentNullException.ThrowIfNull(simulationSettings, nameof(simulationSettings));

        var filtered = timedEvents
            .GroupBy(e => e.Timestamp)
            .OrderBy(g => g.Key)
            .Select(g => g.Last());

        var writer = new StreamWriter(stream);
        var headersSaved = false;
        var csvOptions = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            ShouldQuote = x => ShouldQuote(x, headersSaved)
        };
        var csv = new CsvWriter(writer, csvOptions);

        var headers = GetHeaders(simulationSettings);

        foreach (var heading in headers)
        {
            csv.WriteField(heading);
        }

        csv.NextRecord();
        headersSaved = true;

        foreach (var item in filtered)
        {
            var values = GetValues(item, simulationSettings);
            foreach (var value in values)
            {
                csv.WriteField(value);
            }

            csv.NextRecord();
            csv.Flush();
            writer.Flush();
            stream.Flush();
        }
    }

    private static bool ShouldQuote(ShouldQuoteArgs quoteArgs, bool headersSaved)
    {
        if (!headersSaved)
        {
            return false;
        }

        if (quoteArgs.FieldType == typeof(string) || quoteArgs.FieldType == typeof(DateTime))
        {
            return true;
        }

        return false;
    }

    private static IEnumerable<string> GetHeaders(SimulationSettings simulationSettings)
    {
        yield return "Timestamp";
        yield return "Name";
        yield return "Message";

        yield return "TotalRunningAgents";
        yield return "TotalRunningRequests";
        yield return "TotalRequestsQueue";

        foreach (var agent in simulationSettings.Agents)
        {
            yield return $"RunningAgent_{agent.Name}".Replace(' ', '_');
        }

        foreach (var request in simulationSettings.RequestConfigurations)
        {
            yield return $"RunningRequests_{request.Name}".Replace(' ', '_');
            yield return $"RequestsQueue_{request.Name}".Replace(' ', '_');
        }
    }

    private static IEnumerable<object> GetValues(TimeEventData timeEvent, SimulationSettings simulationSettings)
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
