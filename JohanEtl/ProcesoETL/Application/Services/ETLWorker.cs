using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcesoETL.Core.Configuration;
using ProcesoETL.Core.Interfaces;
using Domain.Models;
using System.Diagnostics;

namespace ProcesoETL.Application.Services;

/// <summary>
/// Background worker service that orchestrates the ETL process
/// </summary>
public class ETLWorker : BackgroundService
{
    private readonly ILogger<ETLWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ETLSettings _settings;

    public ETLWorker(
        ILogger<ETLWorker> logger,
        IServiceProvider serviceProvider,
        IOptions<ETLSettings> settings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ETL Worker Service started at: {time}", DateTimeOffset.Now);

        // Run immediately on startup
        await RunETLProcessAsync(stoppingToken);

        // Then run on scheduled interval
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_settings.RunIntervalMinutes));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunETLProcessAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ETL Worker Service is stopping");
        }
    }

    private async Task RunETLProcessAsync(CancellationToken cancellationToken)
    {
        var overallStopwatch = Stopwatch.StartNew();
        _logger.LogInformation("========== Starting ETL Process ==========");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var pipeline = scope.ServiceProvider.GetRequiredService<ETLPipeline>();
            
            await pipeline.RunAsync(cancellationToken);

            overallStopwatch.Stop();
            _logger.LogInformation(
                "========== ETL Process Completed Successfully in {ElapsedSeconds:F2}s ==========",
                overallStopwatch.Elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            overallStopwatch.Stop();
            _logger.LogError(
                ex,
                "========== ETL Process Failed after {ElapsedSeconds:F2}s ==========",
                overallStopwatch.Elapsed.TotalSeconds);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ETL Worker Service stopped at: {time}", DateTimeOffset.Now);
        return base.StopAsync(cancellationToken);
    }
}
