using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcesoETL.Core.Configuration;
using ProcesoETL.Core.Interfaces;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProcesoETL.Infrastructure.Extractors;

/// <summary>
/// Extractor for REST API data sources
/// </summary>
/// <typeparam name="T">The type of data to extract from API</typeparam>
public class ApiExtractor<T> : IExtractor<T>
{
    private readonly ILogger<ApiExtractor<T>> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    public string Name { get; }

    public ApiExtractor(
        ILogger<ApiExtractor<T>> logger,
        HttpClient httpClient,
        string endpoint,
        string name)
    {
        _logger = logger;
        _httpClient = httpClient;
        _endpoint = endpoint;
        Name = name;
    }

    public async Task<IEnumerable<T>> ExtractAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting API extraction from {Endpoint} for {ExtractorName}", _endpoint, Name);

        try
        {
            var response = await _httpClient.GetAsync(_endpoint);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "API request failed with status {StatusCode} for {Endpoint}",
                    response.StatusCode,
                    _endpoint);
                return Enumerable.Empty<T>();
            }

            var data = await response.Content.ReadFromJsonAsync<List<T>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var records = data ?? new List<T>();

            stopwatch.Stop();
            _logger.LogInformation(
                "Successfully extracted {RecordCount} records from API {Endpoint} in {ElapsedMs}ms",
                records.Count,
                _endpoint,
                stopwatch.ElapsedMilliseconds);

            return records;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "HTTP error extracting data from API {Endpoint}", _endpoint);
            return Enumerable.Empty<T>();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error extracting data from API {Endpoint}", _endpoint);
            throw;
        }
    }
}
