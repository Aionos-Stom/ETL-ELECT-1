using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcesoETL.Core.Configuration;
using ProcesoETL.Core.Interfaces;
using System.Diagnostics;
using System.Globalization;

namespace ProcesoETL.Infrastructure.Extractors;

/// <summary>
/// Extractor for CSV file data sources
/// </summary>
/// <typeparam name="T">The type of data to extract from CSV</typeparam>
public class CsvExtractor<T> : IExtractor<T>
{
    private readonly ILogger<CsvExtractor<T>> _logger;
    private readonly string _filePath;
    public string Name { get; }

    public CsvExtractor(
        ILogger<CsvExtractor<T>> logger,
        string filePath,
        string name)
    {
        _logger = logger;
        _filePath = filePath;
        Name = name;
    }

    public async Task<IEnumerable<T>> ExtractAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting CSV extraction from {FilePath} for {ExtractorName}", _filePath, Name);

        try
        {
            if (!File.Exists(_filePath))
            {
                _logger.LogError("CSV file not found: {FilePath}", _filePath);
                return Enumerable.Empty<T>();
            }

            var records = new List<T>();

            await Task.Run(() =>
            {
                using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    TrimOptions = TrimOptions.Trim
                });

                records = csv.GetRecords<T>().ToList();
            });

            stopwatch.Stop();
            _logger.LogInformation(
                "Successfully extracted {RecordCount} records from {FilePath} in {ElapsedMs}ms",
                records.Count,
                _filePath,
                stopwatch.ElapsedMilliseconds);

            return records;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error extracting data from CSV file {FilePath}", _filePath);
            throw;
        }
    }
}
