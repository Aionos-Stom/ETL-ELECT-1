namespace ProcesoETL.Core.Interfaces;

/// <summary>
/// Interface for staging data storage before final processing
/// </summary>
public interface IStagingService
{
    /// <summary>
    /// Saves extracted data to staging area
    /// </summary>
    Task SaveToStagingAsync<T>(IEnumerable<T> data, string sourceName);
    
    /// <summary>
    /// Loads data from staging area
    /// </summary>
    Task<IEnumerable<T>> LoadFromStagingAsync<T>(string sourceName);
    
    /// <summary>
    /// Clears staging data for a specific source
    /// </summary>
    Task ClearStagingAsync(string sourceName);
}
