using Dzaba.QueueSimulator.Lib;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.RegisterDzabaQueueSimulatorLib();

var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"logs\QueueSimulator.log");
var outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] ({SourceContext}) [{ThreadId}] {Message:lj}{NewLine}{Exception}";

var logger = new LoggerConfiguration()
    .Enrich.WithThreadId()
    .MinimumLevel.Debug()
    .WriteTo.Async(a => a.Console(LogEventLevel.Information, outputTemplate: outputTemplate))
    .WriteTo.Async(a => a.File(logFile,
        rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 8 * 1024 * 1024, retainedFileCountLimit: 15,
        outputTemplate: outputTemplate))
    .CreateLogger();
builder.Services.AddLogging(l => l.AddSerilog(logger, true));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
});

app.UseAuthorization();

app.MapControllers();

app.Run();
