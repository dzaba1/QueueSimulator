using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using Serilog;
using System;
using System.IO;
using Dzaba.QueueSimulator.Lib;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Dzaba.QueueSimulator.Cmd;

internal static class Program
{
    private static readonly string LoggingOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] ({SourceContext}) [{ThreadId}] {Message:lj}{NewLine}{Exception}";

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var services = new ServiceCollection();

            var config = SetupConfiguration(services);
            SetupLogging(services, config);

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

    private static IConfiguration SetupConfiguration(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        return configuration;
    }

    private static void SetupLogging(IServiceCollection services, IConfiguration configuration)
    {
        var customLoggingOptions = configuration.GetSection("CustomLogging").Get<CustomLoggingOptions>();

        var loggerConfig = new LoggerConfiguration()
            .Enrich.WithThreadId();

        loggerConfig.MinimumLevel.Is(customLoggingOptions.MinimumLevel);
        SetupConsoleLogging(loggerConfig, customLoggingOptions);
        SetupFileLogging(loggerConfig, customLoggingOptions);

        var logger = loggerConfig.CreateLogger();
        services.AddLogging(l => l.AddSerilog(logger, true));
    }

    private static void SetupFileLogging(LoggerConfiguration loggerConfig, CustomLoggingOptions customLoggingOptions)
    {
        if (customLoggingOptions.FileLevel != null)
        {
            var dateTimePart = DateTime.Now.ToString("yyyyMMddHHmmss");
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"logs\QueueSimulator_{dateTimePart}.log");

            loggerConfig.WriteTo.Async(a => a.File(logFile,
                rollOnFileSizeLimit: true, fileSizeLimitBytes: 8 * 1024 * 1024,
                restrictedToMinimumLevel: customLoggingOptions.FileLevel.Value,
                outputTemplate: LoggingOutputTemplate));
        }
    }

    private static void SetupConsoleLogging(LoggerConfiguration loggerConfig, CustomLoggingOptions customLoggingOptions)
    {
        if (customLoggingOptions.ConsoleLevel != null)
        {
            loggerConfig.WriteTo.Async(a => a.Console(customLoggingOptions.ConsoleLevel.Value, outputTemplate: LoggingOutputTemplate));
        }
    }
}
