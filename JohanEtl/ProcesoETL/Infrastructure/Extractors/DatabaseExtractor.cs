using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcesoETL.Core.Interfaces;
using System.Diagnostics;

namespace ProcesoETL.Infrastructure.Extractors;

/// <summary>
/// Extractor for relational database data sources
/// </summary>
/// <typeparam name="T">The type of entity to extract</typeparam>
public class DatabaseExtractor<T> : IExtractor<T> where T : class
{
    private readonly ILogger<DatabaseExtractor<T>> _logger;
    private readonly DbContext _context;
    private readonly Func<DbContext, IQueryable<T>>? _queryBuilder;
    public string Name { get; }

    public DatabaseExtractor(
        ILogger<DatabaseExtractor<T>> logger,
        DbContext context,
        string name,
        Func<DbContext, IQueryable<T>>? queryBuilder = null)
    {
        _logger = logger;
        _context = context;
        Name = name;
        _queryBuilder = queryBuilder;
    }

    public async Task<IEnumerable<T>> ExtractAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Starting database extraction for {ExtractorName}", Name);

        try
        {
            IQueryable<T> query = _queryBuilder != null 
                ? _queryBuilder(_context) 
                : _context.Set<T>();

            var records = await query.AsNoTracking().ToListAsync();

            stopwatch.Stop();
            _logger.LogInformation(
                "Successfully extracted {RecordCount} records from database in {ElapsedMs}ms",
                records.Count,
                stopwatch.ElapsedMilliseconds);

            return records;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error extracting data from database for {ExtractorName}", Name);
            throw;
        }
    }
}
