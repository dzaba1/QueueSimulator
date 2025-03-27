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
    string Serialize(IEnumerable<TimeEventData> timedEvents, SimulationPayload simulationPayload);
    void Serialize(Stream stream, IEnumerable<TimeEventData> timedEvents, SimulationPayload simulationPayload);
}

internal sealed class CsvSerializer : ICsvSerializer
{
    private static readonly HashSet<Type> QuoteTypes = new HashSet<Type>
    {
        typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(DateTimeOffset)
    };

    public string Serialize(IEnumerable<TimeEventData> timedEvents, SimulationPayload simulationPayload)
    {
        ArgumentNullException.ThrowIfNull(timedEvents, nameof(timedEvents));
        ArgumentNullException.ThrowIfNull(simulationPayload, nameof(simulationPayload));

        using var stream = new MemoryStream();
        
        Serialize(stream, timedEvents, simulationPayload);

        stream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public void Serialize(Stream stream, IEnumerable<TimeEventData> timedEvents, SimulationPayload simulationPayload)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));
        ArgumentNullException.ThrowIfNull(timedEvents, nameof(timedEvents));
        ArgumentNullException.ThrowIfNull(simulationPayload, nameof(simulationPayload));

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

        var headers = GetHeaders(simulationPayload);

        foreach (var heading in headers)
        {
            csv.WriteField(heading);
        }

        csv.NextRecord();
        headersSaved = true;

        foreach (var item in filtered)
        {
            var values = GetValues(item, simulationPayload);
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

        if (QuoteTypes.Contains(quoteArgs.FieldType))
        {
            return true;
        }

        return false;
    }

    private static IEnumerable<AgentConfiguration> GetAgentConfigurations(SimulationPayload simulationPayload)
    {
        return simulationPayload.SimulationSettings.Agents
            .Where(a => simulationPayload.AgentConfigurations.ShouldObserve(a.Name));
    }

    private static IEnumerable<RequestConfiguration> GetRequestConfigurations(SimulationPayload simulationPayload)
    {
        return simulationPayload.SimulationSettings.RequestConfigurations
            .Where(a => simulationPayload.RequestConfigurations.ShouldObserve(a.Name));
    }

    private static IEnumerable<string> GetHeaders(SimulationPayload simulationPayload)
    {
        if (simulationPayload.SimulationSettings.ReportSettings.CsvSaveTimestampTicks)
        {
            yield return "Timestamp_Ticks";
        }
        else
        {
            yield return "Timestamp";
        }
        
        yield return "Name";
        yield return "Message";

        yield return "TotalRunningAgents";
        yield return "TotalRunningRequests";
        yield return "TotalRequestsQueue";

        if (simulationPayload.SimulationSettings.ReportSettings.IncludeAllRequests)
        {
            foreach (var request in simulationPayload.SimulationSettings.InitialRequests)
            {
                yield return $"AvgFinishedRequestDuration_{request.Name}".Replace(' ', '_');
            }
        }

        foreach (var agent in GetAgentConfigurations(simulationPayload))
        {
            yield return $"RunningAgent_{agent.Name}".Replace(' ', '_');
        }

        foreach (var request in GetRequestConfigurations(simulationPayload))
        {
            yield return $"RunningRequests_{request.Name}".Replace(' ', '_');
            yield return $"RequestsQueue_{request.Name}".Replace(' ', '_');
        }
    }

    private static IEnumerable<object> GetValues(TimeEventData timeEvent, SimulationPayload simulationPayload)
    {
        if (simulationPayload.SimulationSettings.ReportSettings.CsvSaveTimestampTicks)
        {
            yield return timeEvent.Timestamp.Ticks;
        }
        else
        {
            yield return timeEvent.Timestamp;
        }
            
        yield return timeEvent.Name;
        yield return timeEvent.Message;

        yield return timeEvent.RunningAgents.Total;
        yield return timeEvent.RunningRequests.Total;
        yield return timeEvent.RequestsQueue.Total;

        if (simulationPayload.SimulationSettings.ReportSettings.IncludeAllRequests)
        {
            var avgDict = timeEvent.AllRequests
                .Where(r => r.State == RequestState.Finished)
                .GroupBy(r => r.RequestConfiguration, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Average(r => r.RunningDuration().Value));

            foreach (var request in simulationPayload.SimulationSettings.InitialRequests)
            {
                if (avgDict.TryGetValue(request.Name, out var value))
                {
                    yield return value;
                }
                else
                {
                    yield return TimeSpan.Zero;
                }
            }
        }

        var runningAgentDict = timeEvent.RunningAgents.ToDictionary();
        foreach (var agent in GetAgentConfigurations(simulationPayload))
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
        foreach (var request in GetRequestConfigurations(simulationPayload))
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
