﻿using Dzaba.QueueSimulator.Lib.Model;
using Dzaba.QueueSimulator.Lib;
using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Dzaba.QueueSimulator.Cmd;

internal interface IApp
{
    ExitCode Run(FileInfo input, FileInfo output, Format format);
}

internal sealed class App : IApp
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly ILogger<App> logger;
    private readonly ISimulation simulation;
    private readonly ICsvSerializer csvSerializer;
    private readonly ISimulationContext simulationContext;

    public App(ILogger<App> logger,
        ISimulation simulation,
        ICsvSerializer csvSerializer,
        ISimulationContext simulationContext)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(simulation, nameof(simulation));
        ArgumentNullException.ThrowIfNull(csvSerializer, nameof(csvSerializer));
        ArgumentNullException.ThrowIfNull(simulationContext, nameof(simulationContext));

        this.logger = logger;
        this.simulation = simulation;
        this.csvSerializer = csvSerializer;
        this.simulationContext = simulationContext;
    }

    public ExitCode Run(FileInfo input, FileInfo output, Format format)
    {
        try
        {
            var settings = FromJsonFile(input);
            simulationContext.SetSettings(settings);
            var result = simulation.Run(settings);
            SaveResult(result, simulationContext.Payload, output, format);

            return ExitCode.Ok;
        }
        catch (ExitCodeException eEx)
        {
            foreach (var error in eEx.Errors)
            {
                logger.LogError("{ExitCode}: {Error}", error.Key, error.Value);
            }
            return eEx.Errors.First().Key;
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error.");
            return ExitCode.Unknown;
        }
    }

    private void SaveResult(SimulationReport result,
        SimulationPayload simulationPayload,
        FileInfo output,
        Format format)
    {
        using var stream = output.OpenWrite();

        switch (format)
        {
            case Format.Json:
                var json = JsonSerializer.Serialize(result, JsonOptions);
                var writer = new StreamWriter(stream);
                writer.WriteLine(json);
                writer.Flush();
                break;
            case Format.Csv:
                csvSerializer.Serialize(stream, result.Events, simulationPayload);
                using (var statsStream = new FileStream(output.FullName + ".stats.json", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var statsJson = JsonSerializer.Serialize(result.RequestDurationStatistics, JsonOptions);
                    using var statsWriter = new StreamWriter(statsStream);
                    statsWriter.WriteLine(statsJson);
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
