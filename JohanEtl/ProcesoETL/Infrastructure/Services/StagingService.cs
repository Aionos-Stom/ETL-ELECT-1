using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcesoETL.Core.Configuration;
using ProcesoETL.Core.Interfaces;
using System.Text.Json;

namespace ProcesoETL.Infrastructure.Services;

/// <summary>
/// Service for managing staging data storage
/// </summary>
public class StagingService : IStagingService
{
    private readonly ILogger<StagingService> _logger;
    private readonly string _stagingPath;

    public StagingService(
        ILogger<StagingService> logger,
        IOptions<ETLSettings> settings)
    {
        _logger = logger;
        _stagingPath = settings.Value.StagingPath;
        
        // Ensure staging directory exists
        if (!Directory.Exists(_stagingPath))
        {
            Directory.CreateDirectory(_stagingPath);
            _logger.LogInformation("Created staging directory: {StagingPath}", _stagingPath);
        }
    }

    public async Task SaveToStagingAsync<T>(IEnumerable<T> data, string sourceName)
    {
        var fileName = $"{sourceName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(_stagingPath, fileName);

        try
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            await File.WriteAllTextAsync(filePath, json);
            
            _logger.LogInformation(
                "Saved staging data for {SourceName} to {FilePath}",
                sourceName,
                filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving staging data for {SourceName}", sourceName);
            throw;
        }
    }

    public async Task<IEnumerable<T>> LoadFromStagingAsync<T>(string sourceName)
    {
        try
        {
            var files = Directory.GetFiles(_stagingPath, $"{sourceName}_*.json")
                .OrderByDescending(f => f)
                .ToList();

            if (!files.Any())
            {
                _logger.LogWarning("No staging files found for {SourceName}", sourceName);
                return Enumerable.Empty<T>();
            }

            var latestFile = files.First();
            var json = await File.ReadAllTextAsync(latestFile);
            var data = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();

            _logger.LogInformation(
                "Loaded {RecordCount} records from staging file {FilePath}",
                data.Count,
                latestFile);

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading staging data for {SourceName}", sourceName);
            throw;
        }
    }

    public async Task ClearStagingAsync(string sourceName)
    {
        try
        {
            var files = Directory.GetFiles(_stagingPath, $"{sourceName}_*.json");
            
            foreach (var file in files)
            {
                File.Delete(file);
            }

            _logger.LogInformation(
                "Cleared {FileCount} staging files for {SourceName}",
                files.Length,
                sourceName);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing staging data for {SourceName}", sourceName);
            throw;
        }
    }
}
