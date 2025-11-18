namespace ProcesoETL.Core.Interfaces;

/// <summary>
/// Interface for loading data into the analytics database
/// </summary>
public interface IDataLoader
{
    /// <summary>
    /// Loads extracted data into the analytics database
    /// </summary>
    /// <typeparam name="T">The type of data to load</typeparam>
    /// <param name="data">The data to load</param>
    Task LoadAsync<T>(IEnumerable<T> data) where T : class;
    
    /// <summary>
    /// Loads data with identity handling for specific table
    /// </summary>
    Task LoadWithIdentityAsync<T>(IEnumerable<T> data, string tableName, string pkColumn) where T : class;
}
