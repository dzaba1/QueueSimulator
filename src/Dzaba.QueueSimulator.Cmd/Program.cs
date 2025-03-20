using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using Serilog;
using System;
using System.IO;
using Dzaba.QueueSimulator.Lib;
using System.CommandLine;
using System.Threading.Tasks;

namespace Dzaba.QueueSimulator.Cmd;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var services = new ServiceCollection();

            var dateTimePart = DateTime.Now.ToString("yyyyMMddHHmmss");
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"logs\QueueSimulator_{dateTimePart}.log");
            var outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] ({SourceContext}) [{ThreadId}] {Message:lj}{NewLine}{Exception}";
            var logger = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .MinimumLevel.Debug()
                .WriteTo.Console(LogEventLevel.Information, outputTemplate: outputTemplate)
                .WriteTo.File(logFile, rollOnFileSizeLimit: true, fileSizeLimitBytes: 8*1024*1024,
                    outputTemplate: outputTemplate)
                .CreateLogger();
            services.AddLogging(l => l.AddSerilog(logger, true));

            services.RegisterDzabaQueueSimulatorLib();
            services.AddTransient<IApp, App>();

            using var container = services.BuildServiceProvider();

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

            var exitCode = ExitCode.Ok;

            rootCommand.SetHandler((i, o, f) =>
            {
                var app = container.GetRequiredService<IApp>();
                exitCode = app.Run(i, o, f);
            }, inputOption, outputOption, formatOption);

            await rootCommand.InvokeAsync(args);

            return (int)exitCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }
}
