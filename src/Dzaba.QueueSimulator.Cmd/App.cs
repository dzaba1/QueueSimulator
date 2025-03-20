using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using System.Linq;

namespace Dzaba.QueueSimulator.Cmd;

internal interface IApp
{
    ExitCode Run(FileInfo input, FileInfo output, Format format);
}

internal sealed class App : IApp
{
    private readonly ILogger<App> logger;
    private readonly ISimulation simulation;
    private readonly ICsvSerializer csvSerializer;

    public App(ILogger<App> logger,
        ISimulation simulation,
        ICsvSerializer csvSerializer)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(simulation, nameof(simulation));
        ArgumentNullException.ThrowIfNull(csvSerializer, nameof(csvSerializer));

        this.logger = logger;
        this.simulation = simulation;
        this.csvSerializer = csvSerializer;
    }

    public ExitCode Run(FileInfo input, FileInfo output, Format format)
    {
        try
        {
            var settings = FromJsonFile(input);
            var result = simulation.Run(settings);
            SaveResult(result, settings, output, format);

            return ExitCode.Ok;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error.");
            return ExitCode.Unknown;
        }
    }

    private void SaveResult(IEnumerable<TimeEventData> result,
        SimulationSettings simulationSettings,
        FileInfo output,
        Format format)
    {
        using var stream = output.OpenWrite();

        switch (format)
        {
            case Format.Json:
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                };
                JsonSerializer.Serialize(result.ToArray(), jsonOptions);
                break;
            case Format.Csv:
                var csv = csvSerializer.Serialize(result, simulationSettings);
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine(csv);
                }
                break;
            default: throw new ArgumentOutOfRangeException("format", $"Unknown format: {format}");
        }
    }

    private static SimulationSettings FromJsonFile(FileInfo inputJson)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        using var stream = inputJson.OpenRead();

        return JsonSerializer.Deserialize<SimulationSettings>(stream, jsonOptions);
    }
}
