using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcesoETL.Core.Interfaces;
using System.Data;

namespace ProcesoETL.Infrastructure.Services;

/// <summary>
/// Service for loading data into the analytics database
/// </summary>
public class DataLoader : IDataLoader
{
    private readonly ILogger<DataLoader> _logger;
    private readonly DbContext _context;

    public DataLoader(
        ILogger<DataLoader> logger,
        DbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task LoadAsync<T>(IEnumerable<T> data) where T : class
    {
        var list = data.ToList();
        if (!list.Any())
        {
            _logger.LogInformation("No data to load for type {TypeName}", typeof(T).Name);
            return;
        }

        try
        {
            _context.Set<T>().AddRange(list);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation(
                "Successfully loaded {RecordCount} records of type {TypeName}",
                list.Count,
                typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data for type {TypeName}", typeof(T).Name);
            throw;
        }
    }

    public async Task LoadWithIdentityAsync<T>(IEnumerable<T> data, string tableName, string pkColumn) where T : class
    {
        var list = data.ToList();
        if (!list.Any())
        {
            _logger.LogInformation("No data to load for table {TableName}", tableName);
            return;
        }

        try
        {
            // Try normal insert first
            try
            {
                _context.Set<T>().AddRange(list);
                await _context.SaveChangesAsync();
                DetachAll();
                
                _logger.LogInformation(
                    "Successfully loaded {RecordCount} records into {TableName}",
                    list.Count,
                    tableName);
                return;
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                if (inner.IndexOf("identity", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    inner.IndexOf("Cannot insert explicit value for identity column", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var isIdentity = await IsColumnIdentityAsync(tableName, pkColumn);
                    if (!isIdentity) throw;

                    // Retry with IDENTITY_INSERT
                    DetachAll();
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT dbo.[{tableName}] ON;");
                        _context.Set<T>().AddRange(list);
                        await _context.SaveChangesAsync();
                        await _context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT dbo.[{tableName}] OFF;");
                        await transaction.CommitAsync();
                        DetachAll();
                        
                        _logger.LogInformation(
                            "Successfully loaded {RecordCount} records into {TableName} with IDENTITY_INSERT",
                            list.Count,
                            tableName);
                    }
                    catch
                    {
                        try 
                        { 
                            await _context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT dbo.[{tableName}] OFF;"); 
                        } 
                        catch { /* Ignore cleanup errors */ }
                        
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data into table {TableName}", tableName);
            throw;
        }
    }

    private void DetachAll()
    {
        var entries = _context.ChangeTracker.Entries().ToList();
        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    private async Task<bool> IsColumnIdentityAsync(string tableName, string columnName)
    {
        var sql = $@"
            SELECT CASE WHEN c.is_identity = 1 THEN 1 ELSE 0 END
            FROM sys.columns c
            JOIN sys.tables t ON c.object_id = t.object_id
            WHERE t.name = '{tableName}' AND c.name = '{columnName}'";

        var connection = _context.Database.GetDbConnection();
        
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value && Convert.ToInt32(result) == 1;
    }
}
