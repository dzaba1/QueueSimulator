using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using Serilog;
using System;
using System.IO;
using Dzaba.QueueSimulator.Lib;
using Dzaba.QueueSimulator.Lib.Model;
using System.Text.Json;
using System.CommandLine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Dzaba.QueueSimulator.Cmd;

internal static class Program
{
    private static ServiceProvider Container { get; set; }

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var services = new ServiceCollection();

            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"logs\QueueSimulator.log");
            var outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] ({SourceContext}) [{ThreadId}] {Message:lj}{NewLine}{Exception}";
            var logger = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .MinimumLevel.Debug()
                .WriteTo.Console(LogEventLevel.Information, outputTemplate: outputTemplate)
                .WriteTo.File(logFile, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, outputTemplate: outputTemplate)
                .CreateLogger();
            services.AddLogging(l => l.AddSerilog(logger, true));

            services.RegisterDzabaQueueSimulatorLib();

            using var container = services.BuildServiceProvider();
            Container = container;

            var inputOption = new Option<FileInfo>(["--input", "-i"], "JSON simulation settings model.")
            {
                IsRequired = true,
            };

            var outputOption = new Option<FileInfo>(["--output", "-o"], "JSON simulation settings model.")
            {
                IsRequired = true
            };

            var formatOption = new Option<Format>(["--format", "-f"], "JSON simulation settings model.");
            formatOption.SetDefaultValue(Format.Json);

            var rootCommand = new RootCommand("Dzaba Queue Simulator");
            rootCommand.AddOption(inputOption);
            rootCommand.AddOption(outputOption);
            rootCommand.AddOption(formatOption);

            rootCommand.SetHandler((i, o, f) =>
            {
                var settings = FromJsonFile(i);
                var simulation = container.GetRequiredService<ISimulation>();
                var result = simulation.Run(settings);
                SaveResult(result, settings, o, f);
            }, inputOption, outputOption, formatOption);

            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void SaveResult(IEnumerable<TimeEventData> result,
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
                var serializer = Container.GetRequiredService<ICsvSerializer>();
                var csv = serializer.Serialize(result, simulationSettings);
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
